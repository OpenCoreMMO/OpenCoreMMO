using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location;

namespace NeoServer.Game.Common.Contracts.World.Tiles;

public interface ITile : IThing
{
    IItem TopItemOnStack { get; }
    ICreature TopCreatureOnStack { get; }
    bool BlockMissile { get; }
    int ThingsCount { get; }
    bool HasThings { get; }
    public bool ProtectionZone { get; }

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
}