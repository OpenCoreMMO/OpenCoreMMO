using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

/// <summary>
///     Validates whether a player can throw/move an item to a given destination.
///     This centralizes distance and destination-type checks, handling special cases
///     like "teleports", holes, and floor changes that bypass max throw distance rules.
/// </summary>
public interface IItemThrowValidator
{
    /// <summary>
    ///     Validates that the player can throw an item from their position to the target location.
    ///     Returns a successful result if the throw is allowed, or an error describing why it's not.
    /// </summary>
    Result Validate(IPlayer player, Location.Structs.Location fromLocation, Location.Structs.Location toLocation,
        ITile destinationTile);
}
