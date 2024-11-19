using NeoServer.BuildingBlocks.Application;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Common.Texts;
using NeoServer.Game.World.Map;

namespace NeoServer.Modules.Movement.Creature.Walk;

public class WalkService(IMap map, CreatureMovementPacketDispatcher creatureMovementPacketDispatcher) : ISingleton
{
    /// <summary>
    /// Responsible for handling creature walking.
    /// </summary>
    public void Walk(IWalkableCreature creature, Direction nextDirection)
    {
        if (nextDirection == Direction.None) return;

        var nextTile = map.GetNextTile(creature.Location, nextDirection);

        if (creature.Location.Z != 8 && creature.Tile.HasHeight(3))
        {
            var toLocation = creature.Location.GetNextLocation(nextDirection);
            var newDestination = new Location(toLocation.X, toLocation.Y, (byte)(toLocation.Z - 1));

            if (map[newDestination] is IDynamicTile newDestinationTile) nextTile = newDestinationTile;
        }

        if (!creature.Location.IsSurface && nextTile is null)
        {
            var toLocation = creature.Location.GetNextLocation(nextDirection);
            var newDestination = toLocation.AddFloors(1);

            if (map[newDestination] is IDynamicTile newDestinationTile && newDestinationTile.HasHeight(3))
                nextTile = newDestinationTile;
        }

        if (nextTile is IDynamicTile dynamicTile && !(dynamicTile.CanEnter?.Invoke(creature) ?? true))
        {
            creature.CancelWalk();
            OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        if (creature.TileEnterRule.CanEnter(nextTile, creature) && TryMoveCreature(creature, nextTile.Location)) return;

        creature.CancelWalk();
        OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
    }

    private bool TryMoveCreature(ICreature creature, Location toLocation)
    {
        if (creature is not IWalkableCreature walkableCreature) return false;

        if (map[creature.Location] is not IDynamicTile fromTile)
        {
            OperationFailService.Send(creature as IPlayer, InvalidOperation.NotPossible);
            return false;
        }

        var tileDestination = map[toLocation];

        if (tileDestination is not IDynamicTile toTile) //immutable tiles cannot be modified
        {
            OperationFailService.Send(creature as IPlayer, InvalidOperation.NotPossible);
            return false;
        }

        var result = CylinderOperation.MoveCreature(creature, fromTile, toTile, 1, out var cylinder);
        if (result.Succeeded is false) return false;

        walkableCreature.OnMoved(fromTile, toTile, cylinder.TileSpectators);

        creatureMovementPacketDispatcher.Send(walkableCreature, cylinder);

        if (toTile.HasTeleport(out var teleport) && teleport.HasDestination)
        {
            teleport.Teleport(walkableCreature);
            return true;
        }

        tileDestination = map.GetTileDestination(tileDestination);

        if (tileDestination is null || tileDestination.Location == toLocation) return true;

        TryMoveCreature(creature, tileDestination.Location);

        return true;
    }
}