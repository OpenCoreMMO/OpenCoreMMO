using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureTeleportedEventHandler : IGameEventHandler
{
    private readonly IMap map;

    public CreatureTeleportedEventHandler(IMap map)
    {
        this.map = map;
    }

    public void Execute(IWalkableCreature creature, Location location)
    {
        if (creature.Location == location) return;

        var destination = location;

        if (map[location] is not IDynamicTile { FloorDirection: FloorChangeDirection.None } tile)
            tile = FindNeighbourTile(creature, location);

        if (destination == Location.Zero) return;
        if (tile is null || !creature.TileEnterRule.CanEnter(tile, creature)) return;

        map.TryMoveCreature(creature, tile.Location);
    }

    private IDynamicTile FindNeighbourTile(IWalkableCreature creature, Location location)
    {
        foreach (var neighbour in location.Neighbours)
            if (map[neighbour] is IDynamicTile toTile)
                return toTile;

        return null;
    }
}