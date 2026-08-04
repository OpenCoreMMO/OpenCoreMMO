using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Creatures.Services;

public enum MovementValidationFailureReason
{
    None,
    InvalidDirection,
    TileNotFound,
    ProtectionZoneBlocked,
    CanEnterFunctionDenied,
    NotInvitedToHouse,
    TileEnterRuleDenied
}

public record MovementValidationResult(bool IsValid, MovementValidationFailureReason Reason, ITile DestinationTile);

/// <summary>
///     Service responsible for validating creature movement to a destination tile.
///     Centralizes all pre-movement checks to ensure consistency and reusability.
/// </summary>
public class CreatureMovementValidation(IMap map)
{
    /// <summary>
    ///     Validates if a walkable creature can move in the specified direction.
    ///     Performs all necessary checks including tile existence, height adjustments, protection zones, and entry rules.
    /// </summary>
    /// <param name="creature">The creature attempting to move.</param>
    /// <param name="direction">The direction to move in.</param>
    /// <returns>A validation result indicating success or failure with reason and destination tile.</returns>
    public MovementValidationResult CanWalkTo(IWalkableCreature creature, Direction direction)
    {
        if (direction == Direction.None)
            return new MovementValidationResult(false, MovementValidationFailureReason.InvalidDirection, null);

        var nextTile = map.GetNextTile(creature.Location, direction);

        // Handle downward movement when on elevated tiles (e.g., stairs): if not on ground level and current tile has height 3,
        // check if the tile below in the movement direction exists and use it as the next tile.
        if (creature.Location.Z != 8 && creature.Tile.HasHeight(3))
        {
            var toLocation = creature.Location.GetNextLocation(direction);
            var newDestination = new Location(toLocation.X, toLocation.Y, (byte)(toLocation.Z - 1));

            if (map[newDestination] is IDynamicTile newDestinationTile) nextTile = newDestinationTile;
        }

        // Handle upward movement when underground: if not on the surface and no tile found, try moving up a floor
        // if there's a tile with height 3 (e.g., holes leading up).
        if (!creature.Location.IsSurface && nextTile is null)
        {
            var toLocation = creature.Location.GetNextLocation(direction);
            var newDestination = toLocation.AddFloors(1);

            if (map[newDestination] is IDynamicTile newDestinationTile && newDestinationTile.HasHeight(3))
                nextTile = newDestinationTile;
        }

        if (nextTile is null)
            return new MovementValidationResult(false, MovementValidationFailureReason.TileNotFound, null);

        if (nextTile is not IDynamicTile dynamicTile)
            return new MovementValidationResult(false, MovementValidationFailureReason.TileNotFound, nextTile);

        // Prevent players from entering protection zones if blocked (e.g., during combat).
        if (creature is IPlayer player && nextTile.ProtectionZone && player.IsProtectionZoneBlocked)
            return new MovementValidationResult(false, MovementValidationFailureReason.ProtectionZoneBlocked, nextTile);

        // Check tile-specific entry conditions (e.g., house invite, tile rules).
        if (!(dynamicTile.CanEnterFunction?.Invoke(creature) ?? true))
        {
            // Uninvited players on house tiles get a specific cancel message.
            if (dynamicTile.HouseId is > 0 && creature is IPlayer)
            {
                return new MovementValidationResult(false, MovementValidationFailureReason.NotInvitedToHouse,
                    nextTile);
            }

            return new MovementValidationResult(false, MovementValidationFailureReason.CanEnterFunctionDenied,
                nextTile);
        }

        // Use the creature's tile enter rule to check if it can enter the tile.
        if (!creature.TileEnterRule.CanEnter(nextTile, creature))
            return new MovementValidationResult(false, MovementValidationFailureReason.TileEnterRuleDenied, nextTile);

        return new MovementValidationResult(true, MovementValidationFailureReason.None, nextTile);
    }

    /// <summary>
    ///     Validates if a walkable creature can move to a specific location.
    ///     Computes the direction from the creature's current location and validates accordingly.
    /// </summary>
    /// <param name="creature">The creature attempting to move.</param>
    /// <param name="location">The target location.</param>
    /// <returns>A validation result indicating success or failure with reason and destination tile.</returns>
    public MovementValidationResult CanWalkTo(IWalkableCreature creature, Location location)
    {
        if (creature is null || location == Location.Zero)
            return new MovementValidationResult(false, MovementValidationFailureReason.InvalidDirection, null);

        var direction = creature.Location.DirectionTo(location);
        return CanWalkTo(creature, direction);
    }
}