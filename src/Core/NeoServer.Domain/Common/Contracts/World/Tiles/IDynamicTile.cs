using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Common.Contracts.World.Tiles;

public delegate void AddCreatureToTile(ICreature creature, ITile tile);

public interface IDynamicTile : ITile, IHasItem
{
    IGround Ground { get; }
    List<IWalkableCreature> Creatures { get; }
    int CreaturesCount => Creatures != null ? Creatures.Count : 0;
    ushort StepSpeed { get; }

    FloorChangeDirection FloorDirection { get; }
    bool HasAnyCreature { get; }
    MagicField MagicField { get; }

    bool HasBlockPathFinding { get; }
    bool HasHole { get; }
    List<IPlayer> Players { get; }
    Func<ICreature, bool> CanEnterFunction { get; set; }

    /// <summary>House id from OTBM when this tile belongs to a house; otherwise null.</summary>
    uint? HouseId { get; }

    bool HasTeleport(out TeleportItem teleport);

    byte[] GetRaw(IPlayer playerRequesting = null);
    ICreature GetTopVisibleCreature(ICreature creature);
    bool TryGetStackPositionOfItem(IItem item, out byte stackPosition);

    event AddCreatureToTile CreatureAdded;
    IItem[] RemoveAllItems();
    ICreature[] RemoveAllCreatures();
    bool HasCreatureOfType<T>() where T : ICreature;
    void ReplaceGround(IGround ground);
    IItem[] RemoveStaticItems();
    IItem RemoveItem(ushort id);
    void ReplaceItem(ushort fromId, IItem toItem);
    Result<IItem> RemoveTopItem(bool force = false);
    bool HasHeight(int totalHeight);
    void ReplaceItem(IItem fromItem, IItem toItem);
    bool UpdateItemType(IItem fromItem, IItemType toItemType);
    IItem RemoveItem(IItem item);
    IItem RemoveItem(ItemGroup group);
    bool HasCreature(ICreature creature);

    /// <summary>
    ///     Replaces an existing item on the tile by removing all items belonging to the same group
    ///     and then adding the specified item.
    /// </summary>
    /// <param name="item">The item to add, which will replace any existing items of the same group.</param>
    void ReplaceItemByGroup(IItem item);

    /// <summary>Marks this tile as a protection zone. Called by <see cref="Houses.House.LinkTile"/> at attach time.</summary>
    void SetAsProtectionZone();
}