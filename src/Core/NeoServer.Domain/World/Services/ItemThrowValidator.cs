using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.World.Algorithms;
using Location = NeoServer.Domain.Common.Location.Structs.Location;

namespace NeoServer.Domain.World.Services;

/// <summary>
///     Validates whether a player can throw/move an item to a given map destination.
///     Enforces max throw distance while allowing exemptions for teleports, holes,
///     and floor-change tiles where the item is expected to travel far.
/// </summary>
public class ItemThrowValidator(IMap map) : IItemThrowValidator
{
    /// <summary>
    ///     The maximum squared-tile distance a player can throw an item under normal
    ///     circumstances. Mirrors the classic 7.x-era Tibia rule.
    /// </summary>
    private const int MaxThrowDistance = 7;

    /// <summary>
    ///     Validates whether the player can throw an item from <paramref name="fromLocation"/>
    ///     to <paramref name="toLocation"/>.
    /// </summary>
    /// <remarks>
    ///     Validation order:
    ///     <list type="number">
    ///         <item>Must be on the same floor (Z axis).</item>
    ///         <item>Must have clear line-of-sight.</item>
    ///         <item>
    ///             Distance must be within <see cref="MaxThrowDistance"/> unless the destination
    ///             tile is a teleport, hole, or floor-change tile.
    ///         </item>
    ///     </list>
    /// </remarks>
    public Result Validate(IPlayer player, Location fromLocation, Location toLocation,
        IDynamicTile destinationTile)
    {
        if (player is null)
            return Result.NotPossible;

        if (destinationTile is null)
            return Result.NotPossible;

        // Items cannot be thrown across floors directly by a player.
        if (fromLocation.Z != toLocation.Z)
            return Result.Fail(InvalidOperation.NotPossible);

        // Sight must be clear between origin and target.
        if (!SightClear.IsSightClear(map, fromLocation, toLocation, false))
            return Result.Fail(InvalidOperation.NotPossible);

        // If the destination is a special tile (teleport, hole, floor change),
        // the item is allowed to go anywhere — skip distance check.
        if (IsSpecialDestination(destinationTile))
            return Result.Success;

        // Enforce max throw distance for regular tiles.
        if (!IsWithinThrowRange(player.Location, toLocation))
            return Result.Fail(InvalidOperation.TooFar);

        return Result.Success;
    }

    /// <summary>
    ///     Determines if the destination tile has a special property (teleport, hole, or
    ///     floor-change direction) that allows items to bypass the max throw distance rule.
    /// </summary>
    private static bool IsSpecialDestination(IDynamicTile tile)
    {
        // Tile has a teleport item on it.
        if (tile.HasTeleport(out _))
            return true;

        // Tile is a hole (down floor change).
        if (tile.HasHole)
            return true;

        // Tile has any floor-change direction (stairs, ramps, etc.).
        if (tile.FloorDirection != default)
            return true;

        return false;
    }

    /// <summary>
    ///     Checks whether the distance between two locations is within the allowed throw range.
    ///     Uses the Chebyshev distance (max of X and Y deltas).
    /// </summary>
    private static bool IsWithinThrowRange(Location playerLocation, Location toLocation)
    {
        return playerLocation.GetMaxSqmDistance(toLocation) <= MaxThrowDistance;
    }
}
