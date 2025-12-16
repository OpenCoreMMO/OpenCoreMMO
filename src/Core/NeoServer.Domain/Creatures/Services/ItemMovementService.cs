using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Mail;

namespace NeoServer.Domain.Creatures.Services;

public class ItemMovementService(IWalkToMechanism walkToMechanism, IMailService mailService) : IItemMovementService
{
    public Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition, bool walkTo = true)
    {
        if (player is null) return Result<OperationResultList<IItem>>.NotPossible;

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

        if (!item.IsCloseTo(player) && walkTo)
        {
            walkToMechanism.WalkTo(player,
                () => player.MoveItem(item, from, destination, amount, fromPosition, toPosition), item.Location);
            return Result<OperationResultList<IItem>>.Success;
        }

        if (destination is IDynamicTile finalTile && finalTile.HasFlag(TileFlags.MailBox) && !item.IsMailable)
        {
            OperationFailService.Send(player, InvalidOperation.NotPossible);
            return Result<OperationResultList<IItem>>.NotPossible;
        }

        return Move(player, item, from, destination, amount, fromPosition, toPosition);
    }


    public Result<OperationResultList<IItem>> Move(IItem item, IHasItem from, IHasItem destination, byte amount,
        byte fromPosition, byte? toPosition)
    {
        if (!item.CanBeMoved) return Result<OperationResultList<IItem>>.NotPossible;

        var canAdd = destination.CanAddItem(item, amount, toPosition);
        if (!canAdd.Succeeded) return new Result<OperationResultList<IItem>>(canAdd.Reason);

        (destination, toPosition) = GetDestination(from, destination, toPosition);

        var possibleAmountToAdd = destination.PossibleAmountToAdd(item, toPosition);
        if (possibleAmountToAdd == 0) return new Result<OperationResultList<IItem>>(InvalidOperation.NotEnoughRoom);

        var removedItem = RemoveItem(item, from, amount, fromPosition, possibleAmountToAdd);

        var result = AddToDestination(removedItem, from, destination, toPosition);

        if (result.Succeeded && item is IMovableThing movableThing && destination is IThing destinationThing)
            movableThing.OnMoved(destinationThing);

        var amountResult = (byte)Math.Max(0, amount - (int)possibleAmountToAdd);
        return amountResult > 0 ? Move(item, from, destination, amountResult, fromPosition, toPosition) : result;
    }

    private Result<OperationResultList<IItem>> Move(IPlayer player, IItem item, IHasItem from, IHasItem destination,
        byte amount,
        byte fromPosition, byte? toPosition)
    {
        if (!item.CanBeMoved) return Result<OperationResultList<IItem>>.NotPossible;

        if (!item.IsCloseTo(player)) return new Result<OperationResultList<IItem>>(InvalidOperation.TooFar);

        var canAdd = destination.CanAddItem(item, amount, toPosition);
        if (!canAdd.Succeeded) return new Result<OperationResultList<IItem>>(canAdd.Reason);

        (destination, toPosition) = GetDestination(from, destination, toPosition);

        var possibleAmountToAdd = destination.PossibleAmountToAdd(item, toPosition);
        if (possibleAmountToAdd == 0) return new Result<OperationResultList<IItem>>(InvalidOperation.NotEnoughRoom);

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
        return amountResult > 0 ? Move(item, from, destination, amountResult, fromPosition, toPosition) : result;
    }

    private static IItem RemoveItem(IItem item, IHasItem from, byte amount, byte fromPosition, uint possibleAmountToAdd)
    {
        var amountToRemove = item is not ICumulative ? (byte)1 : (byte)Math.Min(amount, possibleAmountToAdd);

        from.RemoveItem(item, amountToRemove, fromPosition, out var removedThing);

        return removedThing;
    }

    private static Result<OperationResultList<IItem>> AddToDestination(IItem thing, IHasItem source,
        IHasItem destination,
        byte? toPosition)
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


    private (IHasItem, byte?) GetDestination(IHasItem source, IHasItem destination,
        byte? toPosition)
    {
        if (source is not IContainer sourceContainer) return (destination, toPosition);
        if (destination is not IContainer) return (destination, toPosition);

        if (destination == source && toPosition is not null &&
            sourceContainer.GetContainerAt(toPosition.Value, out var container))
            return (container, null);

        return (destination, toPosition);
    }
}