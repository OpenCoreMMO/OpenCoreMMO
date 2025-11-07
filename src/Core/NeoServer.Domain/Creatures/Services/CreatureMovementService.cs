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

    /// <summary>
    /// Service responsible for handling creature movement in the game world.
    /// Manages pathfinding, collision detection, special tile interactions like teleports, height changes, and protection zones.
    /// Uses cylinder operations for atomic movement updates and event notifications.
    /// </summary>
    public class CreatureMovementService(IMap map, CylinderOperation cylinderOperation): ICreatureMovementService
    {
        /// <summary>
        /// Attempts to move a creature to a specific location.
        /// If movement fails, sends a failure message to the creature.
        /// </summary>
        /// <param name="creature">The creature to move.</param>
        /// <param name="location">The target location.</param>
        /// <returns>True if movement succeeded, false otherwise.</returns>
        public bool MoveCreature(ICreature creature, Location location)
        {
            if (TryMoveCreature(creature, location)) return true;

            OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
            return false;
        }
        /// <summary>
        /// Moves a walkable creature based on its internal pathfinding logic.
        /// Retrieves the next step direction from the creature and attempts to move it.
        /// </summary>
        /// <param name="creature">The walkable creature to move.</param>
        public void MoveCreature(IWalkableCreature creature)
        {
            var nextDirection = creature.GetNextStep();
            MoveCreature(creature, nextDirection);
        }

        /// <summary>
        /// Attempts to move a walkable creature in the specified direction.
        /// If movement fails, sends a failure message to the creature.
        /// </summary>
        /// <param name="creature">The walkable creature to move.</param>
        /// <param name="nextDirection">The direction to move in.</param>
        /// <returns>True if movement succeeded, false otherwise.</returns>
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

            // Handle downward movement when on elevated tiles (e.g., stairs): if not on ground level and current tile has height 3,
            // check if the tile below in the movement direction exists and use it as the next tile.
            if (creature.Location.Z != 8 && creature.Tile.HasHeight(3))
            {
                var toLocation = creature.Location.GetNextLocation(nextDirection);
                var newDestination = new Location(toLocation.X, toLocation.Y, (byte)(toLocation.Z - 1));

                if (map[newDestination] is IDynamicTile newDestinationTile) nextTile = newDestinationTile;
            }

            // Handle upward movement when underground: if not on the surface and no tile found, try moving up a floor
            // if there's a tile with height 3 (e.g., holes leading up).
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

            // Prevent players from entering protection zones if blocked (e.g., during combat).
            if (creature is IPlayer player && nextTile.ProtectionZone && player.IsProtectionZoneBlocked)
            {
                creature.CancelWalk();
                OperationFailService.Send(creature.CreatureId, TextConstants.YOU_CANNOT_ENTER_PROTECTION_ZONE);
                return false;
            }

            // Check tile-specific entry conditions (e.g., locked doors, special areas).
            if (nextTile is IDynamicTile dynamicTile && !(dynamicTile.CanEnterFunction?.Invoke(creature) ?? true))
            {
                creature.CancelWalk();
                OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
                return false;
            }

            // Use the creature's tile enter rule to check if it can enter the tile, then attempt the actual move.
            if (creature.TileEnterRule.CanEnter(nextTile, creature) &&
                TryMoveCreature(creature, nextTile.Location)) return true;

            creature.CancelWalk();
            return false;
        }

        private bool TryMoveCreature(ICreature creature, Location toLocation)
        {
            if (creature is not IWalkableCreature walkableCreature) return false;

            // Ensure the creature is on a dynamic tile that can be modified.
            if (map[creature.Location] is not IDynamicTile fromTile)
            {
                EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotPossible));
                return false;
            }

            var tileDestination = map[toLocation];

            // Immutable tiles cannot be modified, so movement fails.
            if (tileDestination is not IDynamicTile toTile)
            {
                EventAggregator.Invoke(new ThingMovementFailedInTheMap(creature, InvalidOperation.NotEnoughRoom));
                return false;
            }

            creature.OnMoving(tileDestination);

            // Perform the movement using cylinder operation for atomic updates and spectator notifications.
            var result = cylinderOperation.MoveCreature(creature, fromTile, toTile, 1, out var cylinder);
            if (!result.Succeeded) return false;

            // Notify the creature and spectators of the movement.
            walkableCreature.OnMoved(fromTile, toTile, cylinder.TileSpectators);
            EventAggregator.Invoke(new CreatureMovedInTheMap(walkableCreature, cylinder));

            // Handle teleports: if the destination tile has a teleport, execute it.
            if (toTile.HasTeleport(out var teleport) && teleport.HasDestination)
            {
                teleport.Teleport(walkableCreature);
                return true;
            }

            // Check for special tile destinations (e.g., conveyor belts, portals).
            tileDestination = map.GetTileDestination(tileDestination);

            if (tileDestination is null || tileDestination.Location == toLocation) return true;

            // If there's a redirect destination, recursively attempt to move there.
            TryMoveCreature(creature, tileDestination.Location);

            return true;
        }
}