using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Contracts;

namespace NeoServer.Domain.Common.Contracts.Services;

/// <summary>
///     Centralized service for all item movement operations.
///     Acts as the single entry point that orchestrates validation (throw distance,
///     line-of-sight, walk-to), destination resolution (teleports, holes, floor changes),
///     and the actual item transfer regardless of source type (ground, inventory, container).
/// </summary>
public interface IMapItemMovementService
{
    /// <summary>
    /// Moves an item from one dynamic tile to another, performing necessary validations
    /// and resolving the destination as needed.
    /// </summary>
    /// <param name="player">The player initiating the item movement operation.</param>
    /// <param name="item">The item being moved from the source to the destination.</param>
    /// <param name="from">The tile representing the item's current location.</param>
    /// <param name="destination">The tile representing the item's target location.</param>
    /// <param name="amount">The number of items to move.</param>
    /// <param name="fromPosition">The position index of the item on the source tile.</param>
    /// <param name="toPosition">The optional position index on the destination tile.</param>
    /// <returns>
    /// A result containing the operation result list, which includes processing details
    /// and outcomes of the movement operation.
    /// </returns>
    Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IDynamicTile from,
        ITile destination, byte amount, byte fromPosition, byte? toPosition);

    /// <summary>
    /// Moves an item from the player's inventory or an open container to a map tile,
    /// performing throw validation and destination resolution.
    /// </summary>
    /// <param name="player">The player initiating the item movement operation.</param>
    /// <param name="item">The item being moved.</param>
    /// <param name="from">The inventory or container holding the item.</param>
    /// <param name="destination">The target map tile.</param>
    /// <param name="amount">The number of items to move.</param>
    /// <param name="fromPosition">The slot or index of the item in the source.</param>
    /// <param name="toPosition">The optional position index on the destination tile.</param>
    /// <returns>
    /// A result containing the operation result list with the outcome of the movement.
    /// </returns>
    Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from,
        ITile destination, byte amount, byte fromPosition, byte? toPosition);
}
