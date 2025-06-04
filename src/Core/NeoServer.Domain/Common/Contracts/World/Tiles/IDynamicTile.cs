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
    Ground Ground { get; }
    List<IWalkableCreature> Creatures { get; }
    int CreaturesCount => Creatures != null ? Creatures.Count : 0;
    ushort StepSpeed { get; }

    FloorChangeDirection FloorDirection { get; }
    bool HasCreature { get; }
    IMagicField MagicField { get; }

    bool HasBlockPathFinding { get; }
    bool HasHole { get; }
    List<IPlayer> Players { get; }
    Func<ICreature, bool> CanEnterFunction { get; set; }
    bool HasTeleport(out ITeleport teleport);

    byte[] GetRaw(IPlayer playerRequesting = null);
    ICreature GetTopVisibleCreature(ICreature creature);
    bool TryGetStackPositionOfItem(IItem item, out byte stackPosition);

    event AddCreatureToTile CreatureAdded;
    IItem[] RemoveAllItems();
    ICreature[] RemoveAllCreatures();
    bool HasCreatureOfType<T>() where T : ICreature;
    void ReplaceGround(Ground ground);
    IItem[] RemoveStaticItems();
    IItem RemoveItem(ushort id);
    void ReplaceItem(ushort fromId, IItem toItem);
    Result<IItem> RemoveTopItem(bool force = false);
    bool HasHeight(int totalHeight);
    void ReplaceItem(IItem fromItem, IItem toItem);
    bool UpdateItemType(IItem fromItem, IItemType toItemType);
    IItem RemoveItem(IItem item);
    IItem RemoveItem(ItemGroup group);
}