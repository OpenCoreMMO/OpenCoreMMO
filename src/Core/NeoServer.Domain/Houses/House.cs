using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
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

    public IReadOnlyCollection<IDynamicTile> Tiles => _tiles.AsReadOnly();
    public IReadOnlyDictionary<uint, IItem> Doors => _doors.AsReadOnly();
    public IReadOnlyCollection<IItem> Beds => _beds.AsReadOnly();
    public int TileCount => _tiles.Count;
    public int DoorCount => _doors.Count;
    public int BedCount => _beds.Count;

    public void LinkTile(IDynamicTile tile)
    {
        if (_tiles.Contains(tile))
            throw new InvalidOperationException("Tile already belongs to this house.");

        _tiles.Add(tile);
        tile.CanEnterFunction = c => c is IPlayer p && GetAccessLevel(p) != HouseAccessLevel.NotInvited;

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
        if (OwnerGuid != 0 && player.Id == OwnerGuid)
            return HouseAccessLevel.Owner;

        if (_accessLists.TryGetValue(HouseListId.SubOwnerList, out var subOwnerList) && subOwnerList.IsInList(player))
            return HouseAccessLevel.SubOwner;

        if (_accessLists.TryGetValue(HouseListId.GuestList, out var guestList) && guestList.IsInList(player))
            return HouseAccessLevel.Guest;

        return HouseAccessLevel.NotInvited;
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
            _doors.Clear();
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
        if (GetAccessLevel(caster) < HouseAccessLevel.SubOwner)
            return false;

        if (OwnerGuid != 0 && target.Id == OwnerGuid)
            return false;

        if (caster.Level <= target.Level)
            return false;

        if (!_tiles.Contains(target.Tile))
            return false;

        return true;
    }

    public HouseRentResult PayRent(IPlayer owner, ICoinTypeStore coinTypeStore, DateTime now, uint rentPeriodSeconds)
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
