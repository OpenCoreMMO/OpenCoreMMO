using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Services;

/// <summary>
///     Centralized service for all item movement operations.
///     Acts as the single entry point that orchestrates validation (throw distance,
///     line-of-sight, walk-to), destination resolution (teleports, holes, floor changes),
///     and the actual item transfer regardless of source type (ground, inventory, container).
/// </summary>
public interface ICentralizedItemMovementService
{
    /// <summary>
    ///     Moves an item from any source to an <see cref="IHasItem" /> destination,
    ///     performing all necessary validations and destination resolution.
    /// </summary>
    /// <param name="player">The player performing the move.</param>
    /// <param name="item">The item to move.</param>
    /// <param name="from">The source container (tile, inventory, container).</param>
    /// <param name="destination">The destination container.</param>
    /// <param name="amount">The amount of items to move (for cumulative items).</param>
    /// <param name="fromPosition">The position within the source.</param>
    /// <param name="toPosition">The position within the destination, if applicable.</param>
    Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from,
        IHasItem destination, byte amount, byte fromPosition, byte? toPosition);
}
