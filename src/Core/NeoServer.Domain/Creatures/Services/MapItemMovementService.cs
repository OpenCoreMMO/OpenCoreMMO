using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Mail;
using Location = NeoServer.Domain.Common.Location.Structs.Location;

namespace NeoServer.Domain.Creatures.Services;

/// <summary>
///     Service that handles item movement between map tiles.
///     Executes a single, consistent pipeline:
///     <list type="number">
///         <item>Throw validation (distance, line-of-sight, special tile exemptions)</item>
///         <item>Walk-to mechanism (if the source item is too far from the player)</item>
///         <item>Destination resolution (teleports, holes, floor changes)</item>
///         <item>Core item transfer (remove from the source tile, add to the destination tile)</item>
///     </list>
/// </summary>
public class MapItemMovementService(
    IMap map,
    IWalkToMechanism walkToMechanism,
    IItemThrowValidator itemThrowValidator,
    IMailService mailService) : IMapItemMovementService
{
    /// <summary>
    ///     Moves an item from one map tile to another, performing all validations.
    /// </summary>
    public Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IDynamicTile from,
        ITile destination, byte amount, byte fromPosition, byte? toPosition)
    {
        if (player is null || item is null || !item.CanBeMoved)
            return Result<OperationResultList<IItem>>.NotPossible;

        // --- Floor-level check (player must be on the same floor as the source item) ---
        if (item.Location.Type == LocationType.Ground)
        {
            if (item.Location.Z < player.Location.Z)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.FIRST_GO_UPSTAIRS);
                return Result<OperationResultList<IItem>>.NotPossible;
            }

            if (item.Location.Z > player.Location.Z)
            {
                OperationFailService.Send(player.CreatureId, TextConstants.FIRST_GO_DOWNSTAIRS);
                return Result<OperationResultList<IItem>>.NotPossible;
            }
        }

        // --- Throw validation (distance + line-of-sight + special tile exemptions) ---
        // Validation must happen against the ORIGINAL destination so that special tiles
        // (teleport, hole, floor-change) are recognised before they get resolved away.
        var throwValidation = ValidateThrow(player, item, destination);
        if (throwValidation.Failed)
        {
            SendThrowError(player, throwValidation.Reason);
            return new Result<OperationResultList<IItem>>(throwValidation.Reason);
        }

        // --- Resolve destination tile for map targets ---
        destination = ResolveDestination(destination);

        // --- Walk-to if the source item is not close to the player ---
        if (!item.IsCloseTo(player))
        {
            walkToMechanism.WalkTo(player,
                () => Move(player, item, from, destination, amount, fromPosition, toPosition),
                item.Location);
            return Result<OperationResultList<IItem>>.Success;
        }

        // --- Liquid source (water) / trash holder handling ---
        // Items thrown onto water or trash-holder tiles are consumed: removed from source but not placed on the tile.
        if (destination.HasFlag(TileFlags.TrashHolder))
        {
            EventAggregator.Invoke(new ItemMovedToTrashHolder(item, destination));
            return ConsumeItem(item, from, amount, fromPosition);
        }

        // --- Mail box handling ---
        if (destination.HasFlag(TileFlags.MailBox) && destination is IDynamicTile mailBoxTile)
        {
            if (!item.IsMailable)
            {
                OperationFailService.Send(player, InvalidOperation.NotPossible);
                return Result<OperationResultList<IItem>>.NotPossible;
            }

            return HandleMailBoxMove(player, item, from, mailBoxTile, amount, fromPosition, toPosition);
        }

        // --- Core move ---
        return ExecuteMove(item, from, destination as IDynamicTile, amount, fromPosition, toPosition);
    }

    #region Validation

    /// <summary>
    ///     Validates the throw using <see cref="IItemThrowValidator" /> (distance, line-of-sight,
    ///     special tile exemptions).
    /// </summary>
    private Result ValidateThrow(IPlayer player, IItem item, ITile destination)
    {
        return itemThrowValidator.Validate(player, item.Location, destination.Location, destination);
    }

    /// <summary>
    ///     Sends the appropriate error message to the player based on the validation failure reason.
    /// </summary>
    private static void SendThrowError(IPlayer player, InvalidOperation reason)
    {
        switch (reason)
        {
            case InvalidOperation.TooFar:
                OperationFailService.Send(player.CreatureId, TextConstants.DESTINATION_IS_OUT_OF_REACH);
                break;
            default:
                OperationFailService.Send(player.CreatureId, TextConstants.YOU_CANNOT_THROW_THERE);
                break;
        }
    }

    #endregion

    #region Destination Resolution

    /// <summary>
    ///     Resolves the final destination tile by following teleports, holes, and floor-change
    ///     tiles via <see cref="IMap.GetFinalDestination" />.
    /// </summary>
    private ITile ResolveDestination(ITile destination)
    {
        if (map.GetFinalDestination(destination.Location) is IDynamicTile dynamicTile)
            return dynamicTile;

        return destination;
    }

    #endregion

    #region Core Move Mechanics

    /// <summary>
    ///     Consumes an item by removing it from the source without placing it anywhere.
    ///     Used for water / trash-holder tiles.
    /// </summary>
    private static Result<OperationResultList<IItem>> ConsumeItem(IItem item, IHasItem from, byte amount,
        byte fromPosition)
    {
        from.RemoveItem(item, amount, fromPosition, out _);
        return Result<OperationResultList<IItem>>.Success;
    }

    /// <summary>
    ///     Handles moving an item to a mailbox tile. If the mail send succeeds, the item
    ///     is consumed; otherwise, it falls back to a normal move to the tile.
    /// </summary>
    private Result<OperationResultList<IItem>> HandleMailBoxMove(IPlayer player, IItem item, IHasItem from,
        IHasItem destination, byte amount, byte fromPosition, byte? toPosition)
    {
        var canAdd = destination.CanAddItem(item, amount, toPosition);
        if (!canAdd.Succeeded) return new Result<OperationResultList<IItem>>(canAdd.Reason);

        (destination, toPosition) = ResolveContainerDestination(from, destination, toPosition);

        var possibleAmountToAdd = destination.PossibleAmountToAdd(item, toPosition);
        if (possibleAmountToAdd == 0)
            return new Result<OperationResultList<IItem>>(InvalidOperation.NotEnoughRoom);

        var removedItem = RemoveItem(item, from, amount, fromPosition, possibleAmountToAdd);

        var result = Result<OperationResultList<IItem>>.Success;
        var sendMailResult = Result.NotPossible;

        if (destination is IDynamicTile finalTile && finalTile.HasFlag(TileFlags.MailBox) && item.IsMailable)
        {
            sendMailResult = mailService.Send(player, item);
            if (sendMailResult.Succeeded)
            {
                item.SetNewLocation(Location.Zero);
                result = Result<OperationResultList<IItem>>.Success;
            }
        }

        if (sendMailResult.Failed) result = AddToDestination(removedItem, from, destination, toPosition);

        if (result.Succeeded && item is IMovableThing movableThing && destination is IThing destinationThing)
            movableThing.OnMoved(destinationThing);

        var amountResult = (byte)Math.Max(0, amount - (int)possibleAmountToAdd);
        return amountResult > 0
            ? ExecuteMove(item, from, destination, amountResult, fromPosition, toPosition)
            : result;
    }

    /// <summary>
    ///     Executes the core item move: validates capacity, removes from source, adds to destination.
    ///     Handles cumulative items by recursively moving remaining amounts.
    /// </summary>
    private Result<OperationResultList<IItem>> ExecuteMove(IItem item, IHasItem from,
        IHasItem destination, byte amount, byte fromPosition, byte? toPosition)
    {
        var canAdd = destination.CanAddItem(item, amount, toPosition);
        if (!canAdd.Succeeded) return new Result<OperationResultList<IItem>>(canAdd.Reason);

        (destination, toPosition) = ResolveContainerDestination(from, destination, toPosition);

        var possibleAmountToAdd = destination.PossibleAmountToAdd(item, toPosition);
        if (possibleAmountToAdd == 0)
            return new Result<OperationResultList<IItem>>(InvalidOperation.NotEnoughRoom);

        var removedItem = RemoveItem(item, from, amount, fromPosition, possibleAmountToAdd);

        var result = AddToDestination(removedItem, from, destination, toPosition);

        if (result.Succeeded && item is IMovableThing movableThing && destination is IThing destinationThing)
            movableThing.OnMoved(destinationThing);

        var amountResult = (byte)Math.Max(0, amount - (int)possibleAmountToAdd);
        return amountResult > 0
            ? ExecuteMove(item, from, destination, amountResult, fromPosition, toPosition)
            : result;
    }

    /// <summary>
    ///     Removes the specified amount of an item from the source.
    ///     For non-cumulative items, always removes exactly 1.
    /// </summary>
    private static IItem RemoveItem(IItem item, IHasItem from, byte amount, byte fromPosition,
        uint possibleAmountToAdd)
    {
        var amountToRemove = item is not ICumulative ? (byte)1 : (byte)Math.Min(amount, possibleAmountToAdd);
        from.RemoveItem(item, amountToRemove, fromPosition, out var removedThing);
        return removedThing;
    }

    /// <summary>
    ///     Adds an item to the destination. If the destination returns overflow items
    ///     (e.g., swapping items), those are added back to the source.
    /// </summary>
    private static Result<OperationResultList<IItem>> AddToDestination(IItem thing, IHasItem source,
        IHasItem destination, byte? toPosition)
    {
        var canAdd = destination.CanAddItem(thing, thing.Amount, toPosition);
        if (!canAdd.Succeeded) return new Result<OperationResultList<IItem>>(canAdd.Reason);

        var result = destination.AddItem(thing, toPosition);

        if (!(result.Value?.HasAnyOperation ?? false)) return result;

        foreach (var operation in result.Value.Operations)
            if (operation.Item2 == Operation.Removed)
                source.AddItem(operation.Item1);

        return result;
    }

    /// <summary>
    ///     When both source and destination are containers and the player targets a sub-container
    ///     within the same container, redirects the destination to that sub-container.
    /// </summary>
    private static (IHasItem, byte?) ResolveContainerDestination(IHasItem source, IHasItem destination,
        byte? toPosition)
    {
        if (source is not IContainer sourceContainer) return (destination, toPosition);
        if (destination is not IContainer) return (destination, toPosition);

        if (destination == source && toPosition is not null &&
            sourceContainer.GetContainerAt(toPosition.Value, out var container))
            return (container, null);

        return (destination, toPosition);
    }

    #endregion
}