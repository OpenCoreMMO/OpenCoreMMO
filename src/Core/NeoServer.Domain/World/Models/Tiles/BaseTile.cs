using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.World.Models.Tiles;

public abstract class BaseTile : ITile
{
    protected uint Flags;

    public bool CannotLogout => HasFlag(TileFlags.NoLogout);
    public FloorChangeDirection FloorDirection { get; protected set; } = FloorChangeDirection.None;
    public bool ProtectionZone => HasFlag(TileFlags.ProtectionZone);
    public bool PvpZone => HasFlag(TileFlags.PvpZone);
    public bool NoPvpZone => HasFlag(TileFlags.NoPvpZone);

    public ZoneType Zone
    {
        get
        {
            if (HasFlag(TileFlags.PvpZone)) return ZoneType.Pvp;
            if (HasFlag(TileFlags.ProtectionZone)) return ZoneType.Protection;
            if (HasFlag(TileFlags.NoPvpZone)) return ZoneType.NoPvp;
            if (HasFlag(TileFlags.NoLogout)) return ZoneType.NoLogout;
            return ZoneType.Normal;
        }
    }

    public abstract IItem TopTopItemOnStack { get; }
    public abstract IItem TopDownItemOnStack { get; }
    public abstract ICreature TopCreatureOnStack { get; }
    public abstract int ThingsCount { get; }
    public bool HasThings => ThingsCount > 0;
    public abstract bool TryGetStackPositionOfThing(IPlayer player, IThing thing, out byte stackPosition);

    public abstract byte GetCreatureStackPositionIndex(IPlayer observer);

    public bool HasFlag(TileFlags flag)
    {
        return ((uint)flag & Flags) != 0;
    }

    public bool BlockMissile => HasFlag(TileFlags.BlockProjecTile);

    public Location Location { get; private set; }

    public void SetNewLocation(Location location, bool force = false)
    {
        if (Location != default) throw new InvalidOperationException();
        Location = location;
    }

    public string Name { get; }

    public abstract int ItemsCount { get; }

    public abstract IItem[] AllItems { get; }

    public string GetLookText(bool isClose = false, bool showInternalDetails = false)
    {
        return string.Empty;
    }

    public void Use(IPlayer usedBy)
    {
    }

    protected void SetFlag(TileFlags flag)
    {
        Flags |= (uint)flag;
    }

    protected void RemoveFlag(TileFlags flag)
    {
        Flags &= ~(uint)flag;
    }

    protected void SetTileFlags(IItem item)
    {
        if (item is null) return;

        if (FloorDirection == FloorChangeDirection.None && !item.IsUsable) FloorDirection = item.FloorDirection;

        if (item.Metadata.HasFlag(ItemFlag.Unpassable) && !item.CanBeMoved) SetFlag(TileFlags.ImmovableBlockSolid);

        if (item.Metadata.HasFlag(ItemFlag.BlockPathFind)) SetFlag(TileFlags.BlockPath);

        if (item.Metadata.HasFlag(ItemFlag.HasHeight)) SetFlag(TileFlags.HasHeight);

        if (item.Metadata.HasFlag(ItemFlag.Unpassable) && item is not MagicField)
        {
            SetFlag(TileFlags.NoFieldBlockPath);

            if (!item.CanBeMoved) SetFlag(TileFlags.ImmovableNoFieldBlockPath);
        }

        if (item.Metadata.HasFlag(ItemFlag.BlockProjectTile)) SetFlag(TileFlags.BlockProjecTile);

        if (item.Metadata.Attributes.TryGetAttribute(ItemAttribute.BlockProjectTile, out int value) && value == 1)
            SetFlag(TileFlags.BlockProjecTile);

        if (item is TeleportItem) SetFlag(TileFlags.Teleport);

        if (item is MagicField) SetFlag(TileFlags.MagicField);

        // if (item->getMailbox()) { //todo
        //     setFlag(TILESTATE_MAILBOX);
        // }

        // if (item->getTrashHolder()) { //todo
        //     setFlag(TILESTATE_TRASHHOLDER);
        // }

        if (item.Metadata.HasFlag(ItemFlag.Unpassable)) SetFlag(TileFlags.Unpassable);

        // if (item->getBed()) { //todo
        //     setFlag(TILESTATE_BED);
        // }


        if (item is Depot.Depot) SetFlag(TileFlags.Depot);

        if (item.Metadata.HasFlag(ItemFlag.Hangable)) //todo: might be wrong
            SetFlag(TileFlags.SupportsHangable);
    }

    protected void ResetTileFlags(params IItem[] items)
    {
        FloorDirection = FloorChangeDirection.None;
        RemoveFlag(TileFlags.ImmovableBlockSolid);
        RemoveFlag(TileFlags.BlockPath);
        RemoveFlag(TileFlags.ImmovableNoFieldBlockPath);
        RemoveFlag(TileFlags.BlockProjecTile);
        RemoveFlag(TileFlags.Teleport);
        RemoveFlag(TileFlags.MagicField);
        RemoveFlag(TileFlags.Unpassable);
        RemoveFlag(TileFlags.Depot);
        RemoveFlag(TileFlags.SupportsHangable);
        RemoveFlag(TileFlags.MailBox);
        RemoveFlag(TileFlags.TrashHolder);
        RemoveFlag(TileFlags.Bed);

        foreach (var item in items) SetTileFlags(item);
    }
}