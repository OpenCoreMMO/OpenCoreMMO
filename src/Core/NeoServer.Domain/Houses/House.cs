using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Houses.AccessList;

namespace NeoServer.Domain.Houses;

public class House
{
    private readonly List<IDynamicTile> _tiles = new();
    private readonly Dictionary<uint, IItem> _doors = new();
    private readonly List<IItem> _beds = new();
    private readonly Dictionary<uint, HouseAccessList> _accessLists = new();

    public uint Id { get; init; }
    public string Name { get; set; }
    public ushort TownId { get; set; }
    public uint Rent { get; set; }
    public DateTime? PaidUntil { get; set; }
    private byte _payRentWarnings;
    public byte PayRentWarnings
    {
        get => _payRentWarnings;
        set => _payRentWarnings = Math.Min(value, (byte)7);
    }
    public uint OwnerGuid { get; set; }
    public string OwnerName { get; set; }
    public int OwnerAccountId { get; set; }
    public Location? EntryPosition { get; set; }

    /// <summary>
    ///     When true, only players with an active Premium Account may exercise
    ///     sub-owner rights. Free-account characters keep their slot on the
    ///     sub-owner list but are inactive until premium is restored.
    /// </summary>
    public bool RequirePremiumForSubOwners { get; set; } = true;

    public IReadOnlyCollection<IDynamicTile> Tiles => _tiles.AsReadOnly();
    public IReadOnlyDictionary<uint, IItem> Doors => _doors.AsReadOnly();
    public IReadOnlyCollection<IItem> Beds => _beds.AsReadOnly();
    public int TileCount => _tiles.Count;
    public int DoorCount => _doors.Count;
    public int BedCount => _beds.Count;

    public List<IItem> PickupableItems
    {
        get
        {
            var items = new List<IItem>();
            foreach (var tile in _tiles)
            {
                if (tile.AllItems is null) continue;
                foreach (var item in tile.AllItems)
                {
                    if (item is not null && item.IsPickupable)
                        items.Add(item);
                }
            }
            return items;
        }
    }

    public void LinkTile(IDynamicTile tile)
    {
        if (_tiles.Contains(tile))
            throw new InvalidOperationException("Tile already belongs to this house.");

        // Assumption: house linking is the sole writer of CanEnterFunction on tiles.
        // A non-null CanEnterFunction on a different house's tile means it was already claimed.
        // Phase 2 should upgrade this to a HouseId-based check once map<->DB house ids are reconciled.
        if (tile.CanEnterFunction is not null)
            throw new InvalidOperationException("Tile already belongs to another house.");

        _tiles.Add(tile);
        tile.SetAsProtectionZone();
        tile.CanEnterFunction = c => c is IPlayer p && (IsInvited(p) || p.Group?.Access == true);

        if (EntryPosition is null)
            EntryPosition = tile.Location;
    }

    public void LinkDoor(uint doorId, IItem door)
    {
        _doors[doorId] = door;
    }

    public void LinkBed(IItem bed)
    {
        _beds.Add(bed);
    }

    public HouseAccessLevel GetAccessLevel(IPlayer player)
    {
        if (player.Group?.Access == true ||
            player.Group?.FlagIsEnabled(PlayerFlag.CanEditHouses) == true)
            return HouseAccessLevel.Owner;

        if (OwnerGuid != 0 && player.Id == OwnerGuid)
            return HouseAccessLevel.Owner;

        if (_accessLists.TryGetValue(HouseListId.SubOwnerList, out var subOwnerList) &&
            subOwnerList.IsInList(player) &&
            IsActiveSubOwner(player))
            return HouseAccessLevel.SubOwner;

        if (_accessLists.TryGetValue(HouseListId.GuestList, out var guestList) && guestList.IsInList(player))
            return HouseAccessLevel.Guest;

        return HouseAccessLevel.NotInvited;
    }

    /// <summary>
    ///     Whether the player may exercise sub-owner rights. Free-account characters
    ///     keep their slot on the sub-owner list but lose all sub-owner abilities
    ///     until premium is restored.
    /// </summary>
    private bool IsActiveSubOwner(IPlayer player) =>
        !RequirePremiumForSubOwners || player.HasPremiumTime;

    public bool CanUseDoor(IPlayer player, uint doorId)
    {
        _doors.TryGetValue(doorId, out var door);
        return CanUseDoor(player, door?.Location ?? default, doorId);
    }

    /// <summary>
    ///     Invited players may open/close the house entry door. Internal doors require
    ///     sub-owner access or an explicit per-door list entry.
    /// </summary>
    public bool CanUseDoor(IPlayer player, Location doorLocation, uint? doorId = null)
    {
        if (GetAccessLevel(player) >= HouseAccessLevel.SubOwner)
            return true;

        if (IsInvited(player) && IsEntryDoor(doorLocation, doorId))
            return true;

        if (doorId is null)
            return false;

        var list = GetAccessList(doorId.Value);
        return list is not null && list.IsInList(player);
    }

    /// <summary>
    ///     The entry door is the house door on or adjacent to <see cref="EntryPosition"/>
    ///     (the kick/exit tile). When several doors qualify, the closest one wins.
    /// </summary>
    public bool IsEntryDoor(Location doorLocation, uint? doorId = null)
    {
        if (EntryPosition is null)
            return false;

        var entry = EntryPosition.Value;
        if (doorLocation.Z != entry.Z)
            return false;

        var entryDoorId = FindEntryDoorId();
        if (entryDoorId is not null)
        {
            if (doorId is not null)
                return doorId == entryDoorId;

            if (_doors.TryGetValue(entryDoorId.Value, out var entryDoor) && entryDoor is not null)
                return entryDoor.Location == doorLocation;
        }

        return doorLocation.GetMaxSqmDistance(entry) <= 1;
    }

    private uint? FindEntryDoorId()
    {
        if (EntryPosition is null)
            return null;

        var entry = EntryPosition.Value;
        uint? bestId = null;
        var bestDistance = int.MaxValue;

        foreach (var (id, door) in _doors)
        {
            if (door is null)
                continue;

            var location = door.Location;
            if (location.Z != entry.Z)
                continue;

            var distance = location.GetMaxSqmDistance(entry);
            if (distance > 1)
                continue;

            if (distance > bestDistance)
                continue;

            if (distance < bestDistance || bestId is null || id < bestId)
            {
                bestDistance = distance;
                bestId = id;
            }
        }

        return bestId;
    }

    public bool IsInvited(IPlayer player)
    {
        return GetAccessLevel(player) != HouseAccessLevel.NotInvited;
    }

    public bool CanEnter(ICreature creature)
    {
        return creature is IPlayer player && IsInvited(player);
    }

    public bool CanEditAccessList(uint listId, IPlayer player)
    {
        var level = GetAccessLevel(player);

        if (listId == HouseListId.SubOwnerList)
            return level == HouseAccessLevel.Owner;

        if (listId == HouseListId.GuestList || HouseListId.IsDoorList(listId))
            return level >= HouseAccessLevel.SubOwner;

        return false;
    }

    public HouseAccessList GetAccessList(uint listId)
    {
        return _accessLists.GetValueOrDefault(listId);
    }

    public void SetAccessList(uint listId, HouseAccessList list)
    {
        if (list is not null)
            _accessLists[listId] = list;
        else
            _accessLists.Remove(listId);
    }

    public void SetNewOwner(uint guid, string name, int accountId, bool updatePaidUntil, DateTime now, uint rentPeriodSeconds)
    {
        if (guid != 0 && guid == OwnerGuid)
        {
            if (updatePaidUntil)
            {
                PaidUntil = now.AddSeconds(rentPeriodSeconds);
                PayRentWarnings = 0;
            }
            return;
        }

        if (OwnerGuid != 0)
        {
            _accessLists.Clear();
        }

        OwnerGuid = guid;
        OwnerName = guid != 0 ? name : string.Empty;
        OwnerAccountId = guid != 0 ? accountId : 0;

        if (updatePaidUntil && guid != 0)
        {
            PaidUntil = now.AddSeconds(rentPeriodSeconds);
            PayRentWarnings = 0;
        }
    }

    public bool CanKick(IPlayer caster, IPlayer target)
    {
        if (!_tiles.Contains(target.Tile))
            return false;

        if (GetAccessLevel(caster) < GetAccessLevel(target))
            return false;

        if (target.Group?.FlagIsEnabled(PlayerFlag.CanEditHouses) == true)
            return false;

        return true;
    }

    /// <summary>
    ///     Returns players currently inside the house who are no longer invited.
    ///     Used after guest/subowner list changes to evict removed occupants.
    /// </summary>
    public List<IPlayer> GetUninvitedOccupants()
    {
        var uninvited = new List<IPlayer>();

        foreach (var tile in _tiles)
        {
            if (tile.Players is null)
            {
                continue;
            }

            foreach (var occupant in tile.Players)
            {
                if (occupant is null || IsInvited(occupant))
                {
                    continue;
                }

                uninvited.Add(occupant);
            }
        }

        return uninvited;
    }
    // Rent is collected from the player's bank balance (owner.Bank.Debit). A coin-store parameter
    // was intentionally removed (Phase 1); if a future phase needs coin-specific rent, re-introduce
    // it at the service layer, not the aggregate.
    public HouseRentResult PayRent(IPlayer owner, DateTime now, uint rentPeriodSeconds)
    {
        if (OwnerGuid == 0) return HouseRentResult.NotDue;
        if (Rent == 0) return HouseRentResult.NotDue;
        if (PaidUntil.HasValue && PaidUntil.Value > now) return HouseRentResult.NotDue;

        if (owner.BankAmount >= Rent)
        {
            owner.Bank.Debit(Rent);
            PaidUntil = now.AddSeconds(rentPeriodSeconds);
            PayRentWarnings = 0;
            return HouseRentResult.Paid;
        }

        PayRentWarnings = (byte)(PayRentWarnings + 1);

        if (PayRentWarnings >= 7)
        {
            SetNewOwner(0, null, 0, false, now, rentPeriodSeconds);
            return HouseRentResult.Evicted;
        }

        return HouseRentResult.Warned;
    }
}
