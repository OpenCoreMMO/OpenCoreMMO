using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Contracts.Services;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureTeleportedEventHandler : IGameEventHandler
{
    private readonly IMap map;
    private readonly IStaticToDynamicTileService staticToDynamicTileService;

    public CreatureTeleportedEventHandler(IMap map, IStaticToDynamicTileService staticToDynamicTileService = null)
    {
        this.map = map;
        this.staticToDynamicTileService = staticToDynamicTileService;
    }

    public void Execute(IWalkableCreature creature, Location location)
    {
        if (creature.Location == location) return;

        // Get tile at destination. If static, try converting to dynamic.
        ITile tile = map[location];

        if (tile is IStaticTile && staticToDynamicTileService is not null)
        {
            tile = staticToDynamicTileService.TransformIntoDynamicTile(tile);
        }

        // Fallback to a neighbouring tile only if we still couldn't resolve a dynamic tile.
        if (tile is not IDynamicTile dynamicTile)
        {
            dynamicTile = FindNeighbourTile(creature, location);
        }

        if (dynamicTile is null) return;

        // Forced move so teleports always land, even on blocked tiles.
        map.TryMoveCreatureForced(creature, dynamicTile.Location);
    }

    private IDynamicTile FindNeighbourTile(IWalkableCreature creature, Location location)
    {
        foreach (var neighbour in location.Neighbours)
        {
            var tile = map[neighbour];
            if (tile is IStaticTile && staticToDynamicTileService is not null)
            {
                tile = staticToDynamicTileService.TransformIntoDynamicTile(tile);
            }

            if (tile is IDynamicTile toTile)
                return toTile;
        }

        return null;
    }
}
