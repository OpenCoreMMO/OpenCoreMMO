using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Creatures.Players;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player;

public class PlayerHand
{
    private readonly IPlayer _player;

    public PlayerHand(IPlayer player)
    {
        _player = player;
    }

    public Result<OperationResultList<IItem>> Move(IItem item, IHasItem from, IHasItem destination, byte amount,
        byte fromPosition, byte? toPosition)
    {
        if (!item.CanBeMoved) return Result<OperationResultList<IItem>>.NotPossible;

        if (!item.IsCloseTo(_player)) return new Result<OperationResultList<IItem>>(InvalidOperation.TooFar);

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

    public Result<OperationResultList<IItem>> PickItemFromGround(IItem item, ITile tile, byte amount = 1)
    {
        if (tile is not IDynamicTile fromTile) return Result<OperationResultList<IItem>>.NotPossible;

        var topItemOnStackIsPickupable = tile.TopDownItemOnStack?.IsPickupable ?? false;

        if (!topItemOnStackIsPickupable) return Result<OperationResultList<IItem>>.NotPossible;
        if (_player.Inventory.BackpackSlot is not { } backpack) return Result<OperationResultList<IItem>>.NotPossible;

        if (tile.TopDownItemOnStack != item) return Result<OperationResultList<IItem>>.NotPossible;

        return Move(tile.TopDownItemOnStack, fromTile, backpack, amount, 0, 0);
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

        if (!result.Value.HasAnyOperation) return result;

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