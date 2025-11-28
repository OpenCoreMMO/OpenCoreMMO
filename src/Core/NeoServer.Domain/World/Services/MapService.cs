using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World.Services;

public class MapService(IMap map, ICreatureMovementService creatureMovementService) : IMapService
{
    public void ReplaceGround(Location location, IGround ground)
    {
        if (map[location] is not DynamicTile tile) return;
        tile.ReplaceGround(ground);

        if (!tile.HasHole) return;

        var finalTile = GetFinalTile(location);

        if (finalTile is not DynamicTile toTile) return;

        var removedItems = tile.RemoveAllItems();
        var removedCreatures = tile.RemoveAllCreatures();

        toTile.AddItems(removedItems);

        foreach (var removedCreature in removedCreatures)
            creatureMovementService.MoveCreature(removedCreature, toTile.Location);
    }

    public ITile GetFinalTile(Location location)
    {
        var toTile = map[location];
        if (toTile is not IDynamicTile destination) return toTile;

        return destination.HasHole ? GetFinalTile(destination.Location.AddFloors(1)) : toTile;
    }

    public bool GetNeighbourAvailableTile(Location location, ICreature creature, ITileEnterRule rule,
        out ITile foundTile)
    {
        foundTile = null;

        foreach (var neighbour in location.Neighbours)
        {
            if (map[neighbour] is not IDynamicTile tile) continue;
            if (!rule.ShouldIgnore(tile, creature)) continue;

            foundTile = tile;
            return true;
        }

        return false;
    }
}