using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Map;

namespace NeoServer.Domain.Creatures.Services;

public interface ICreatureMovementService
{
    bool MoveCreature(IWalkableCreature creature, Direction nextDirection);
    void MoveCreature(IWalkableCreature creature);
    bool MoveCreature(ICreature creature, Location location);
}

public class CreatureMovementService(IMap map, CylinderOperation cylinderOperation): ICreatureMovementService
{
    public bool MoveCreature(ICreature creature, Location location)
    {
        if (TryMoveCreature(creature, location)) return true;
        
        OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
        return false;
    }
    public void MoveCreature(IWalkableCreature creature)
    {
        var nextDirection = creature.GetNextStep();
        MoveCreature(creature, nextDirection);
    }

    public bool MoveCreature(IWalkableCreature creature, Direction nextDirection)
    {
        if (TryMoveCreature(creature, nextDirection)) return true;
        
        OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
        return false;
    }

    private bool TryMoveCreature(IWalkableCreature creature, Direction nextDirection)
    {
        if (nextDirection == Direction.None) return false;

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

        if (nextTile is null)
        {
            creature.CancelWalk();
            return false;
        }

        if (creature is IPlayer player && nextTile.ProtectionZone && player.IsProtectionZoneBlocked)
        {
            creature.CancelWalk();
            OperationFailService.Send(creature.CreatureId, TextConstants.YOU_CANNOT_ENTER_PROTECTION_ZONE);
            return false;
        }

        if (nextTile is IDynamicTile dynamicTile && !(dynamicTile.CanEnterFunction?.Invoke(creature) ?? true))
        {
            creature.CancelWalk();
            OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
            return false;
        }

        if (creature.TileEnterRule.CanEnter(nextTile, creature) &&
            TryMoveCreature(creature, nextTile.Location)) return true;

        creature.CancelWalk();
        return false;
    }

    private bool TryMoveCreature(ICreature creature, Location toLocation)
    {
        if (creature is not IWalkableCreature walkableCreature) return false;

        if (map[creature.Location] is not IDynamicTile fromTile)
        {
            EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotPossible));
            return false;
        }

        var tileDestination = map[toLocation];

        if (tileDestination is not IDynamicTile toTile) //immutable tiles cannot be modified
        {
            EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotEnoughRoom));
            return false;
        }

        var result = cylinderOperation.MoveCreature(creature, fromTile, toTile, 1, out var cylinder);
        if (!result.Succeeded) return false;

        walkableCreature.OnMoved(fromTile, toTile, cylinder.TileSpectators);
        EventAggregator.Invoke(new CreatureMovedInTheMap(walkableCreature, cylinder));

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