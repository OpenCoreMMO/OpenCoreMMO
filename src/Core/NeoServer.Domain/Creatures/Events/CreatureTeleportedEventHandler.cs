using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Creatures.Events;

public class CreatureTeleportedEventHandler(
    IMap map,
    ICreatureMovementService creatureMovementService,
    IStaticToDynamicTileService staticToDynamicTileService)
    : IGameEventHandler
{
    public void Execute(IWalkableCreature creature, Location location)
    {
        if (creature.Location == location) return;

        if (map[location] is StaticTile staticTile) staticToDynamicTileService.TransformIntoDynamicTile(staticTile);

        if (map[location] is DynamicTile dynamicTile)
            creatureMovementService.MoveCreature(creature, dynamicTile.Location, true, true);
    }
}