using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Creatures.Services;

public interface ICreatureMovementService
{
    bool MoveCreature(IWalkableCreature creature, Direction nextDirection);
    void MoveCreature(IWalkableCreature creature);
    bool MoveCreature(ICreature creature, Location location, bool forced = false, bool isTeleport = false);
}

/// <summary>
///     Service responsible for handling creature movement in the game world.
///     Manages pathfinding, collision detection, special tile interactions like teleports, height changes, and protection
///     zones.
///     Uses cylinder operations for atomic movement updates and event notifications.
/// </summary>
public class CreatureMovementService(
    IMap map,
    CylinderOperation cylinderOperation,
    CreatureMovementValidation movementValidation,
    IStaticToDynamicTileService staticToDynamicTileService) : ICreatureMovementService
{
    /// <summary>
    ///     Attempts to move a creature to a specific location.
    ///     If movement fails, sends a failure message to the creature.
    /// </summary>
    /// <param name="creature">The creature to move.</param>
    /// <param name="location">The target location.</param>
    /// <param name="forced"></param>
    /// <param name="isTeleport"></param>
    /// <returns>True if movement succeeded, false otherwise.</returns>
    public bool MoveCreature(ICreature creature, Location location, bool forced = false, bool isTeleport = false)
    {
        if (TryMoveCreature(creature, location, forced, isTeleport)) return true;

        OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
        return false;
    }

    /// <summary>
    ///     Moves a walkable creature based on its internal pathfinding logic.
    ///     Retrieves the next step direction from the creature and attempts to move it.
    /// </summary>
    /// <param name="creature">The walkable creature to move.</param>
    public void MoveCreature(IWalkableCreature creature)
    {
        var nextDirection = creature.GetNextStep();
        MoveCreature(creature, nextDirection);
    }

    /// <summary>
    ///     Attempts to move a walkable creature in the specified direction.
    ///     If movement fails, sends a failure message to the creature.
    /// </summary>
    /// <param name="creature">The walkable creature to move.</param>
    /// <param name="nextDirection">The direction to move in.</param>
    /// <returns>True if movement succeeded, false otherwise.</returns>
    public bool MoveCreature(IWalkableCreature creature, Direction nextDirection)
    {
        var validation = movementValidation.CanWalkTo(creature, nextDirection);
        if (!validation.IsValid)
        {
            creature.CancelWalk();
            OperationFailService.Send(creature.CreatureId, GetFailureMessage(validation.Reason));
            return false;
        }

        return TryMoveCreature(creature, validation.DestinationTile.Location);
    }

    private bool TryMoveCreature(ICreature creature, Location toLocation, bool forced = false, bool isTeleport = false)
    {
        if (creature is not IWalkableCreature walkableCreature) return false;

        // Ensure the creature is on a dynamic tile that can be modified.
        if (map[creature.Location] is not DynamicTile fromTile)
        {
            EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotPossible));
            return false;
        }

        var tileDestination = map[toLocation];

        if (tileDestination is StaticTile staticTile)
        {
            staticToDynamicTileService.TransformIntoDynamicTile(staticTile);
            tileDestination = map[toLocation];
        }

        // Immutable tiles cannot be modified, so movement fails.
        if (tileDestination is not IDynamicTile toTile)
        {
            EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotEnoughRoom));
            return false;
        }

        creature.OnMoving(tileDestination);

        // Perform the movement using cylinder operation for atomic updates and spectator notifications.
        var result = cylinderOperation.MoveCreature(creature, fromTile, toTile, 1, forced, out var cylinder);
        if (!result.Succeeded) return false;

        // Notify the creature and spectators of the movement.
        walkableCreature.OnMoved(fromTile, toTile, cylinder.TileSpectators);
        EventAggregator.Invoke(new CreatureMovedInTheMap(walkableCreature, cylinder));

        // Handle teleports: if the destination tile has a teleport, execute it.
        if (toTile.HasTeleport(out var teleport) && teleport.HasDestination)
        {
            TryMoveCreature(creature, teleport.Destination, true, true);
            return true;
        }

        // Check for special tile destinations (e.g., conveyor belts, portals).
        tileDestination = map.GetTileDestination(tileDestination);

        if (tileDestination is null || tileDestination.Location == toLocation) return true;

        //when creatures are sent to another place from a teleport, they should not be moved again to another place.
        if (isTeleport) return true;

        // If there's a redirect destination, recursively attempt to move there.
        TryMoveCreature(creature, tileDestination.Location, true);

        return true;
    }

    private static string GetFailureMessage(MovementValidationFailureReason reason)
    {
        return reason switch
        {
            MovementValidationFailureReason.ProtectionZoneBlocked => TextConstants.YOU_CANNOT_ENTER_PROTECTION_ZONE,
            _ => TextConstants.NOT_POSSIBLE
        };
    }
}