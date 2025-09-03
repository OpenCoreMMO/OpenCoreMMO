using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.Common.Contracts.World.Tiles;

public interface ITile : IThing
{
    int ItemsCount { get; }
    IItem[] AllItems { get; }
    IItem TopTopItemOnStack { get; }
    IItem TopDownItemOnStack { get; }
    ICreature TopCreatureOnStack { get; }
    bool BlockMissile { get; }
    int ThingsCount { get; }
    bool HasThings { get; }
    public bool ProtectionZone { get; }
    bool PvpZone { get; }
    bool NoPvpZone { get; }
    ZoneType Zone { get; }

    /// <summary>
    ///     check whether tile is 1 sqm distant to destination tile
    /// </summary>
    /// <returns></returns>
    public bool IsNextTo(ITile dest)
    {
        return Location.IsNextTo(dest.Location);
    }

    bool TryGetStackPositionOfThing(IPlayer player, IThing thing, out byte stackPosition);
    byte GetCreatureStackPositionIndex(IPlayer observer);
    bool HasFlag(TileFlags flag);

    public bool CanEnter(ICreature creature)
    {
        if (creature is not IWalkableCreature walkableCreature) return false;
        if (!walkableCreature.TileEnterRule.CanEnter(this, creature)) return false;
        return true;
    }

    IItem GetItemByIndex(int index);
}