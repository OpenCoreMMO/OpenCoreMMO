using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.SafeTrade.Operations;

namespace NeoServer.Domain.SafeTrade.Validations;

internal static class TradeExchangeValidation
{
    private const string DEFAULT_ERROR_MESSAGE = "Trade could not be completed.";

    /// <summary>
    ///     Performs a safe trade by checking if both players have enough inventory space to add the requested items.
    /// </summary>
    /// <param name="playerRequested">The player who is being requested for an item.</param>
    /// <param name="playerRequesting">The player who is requesting an item.</param>
    /// <param name="itemFromPlayerRequested">The item requested from the playerRequested.</param>
    /// <param name="itemFromPlayerRequesting">The item being offered by the playerRequesting.</param>
    /// <returns>A SafeTradeError enum value indicating whether the trade was successful or not.</returns>
    public static SafeTradeError CanPerformTrade(IPlayer playerRequested, IItem itemFromPlayerRequesting,
        IPlayer playerRequesting,
        IItem itemFromPlayerRequested)
    {
        if (itemFromPlayerRequesting is not Houses.HouseTransferItem)
        {
            var firstPlayerCanAddItem = CanAddItem(playerRequested, itemFromPlayerRequesting, itemFromPlayerRequested);
            if (firstPlayerCanAddItem is not SafeTradeError.None)
            {
                OperationFailService.Send(playerRequesting.CreatureId, DEFAULT_ERROR_MESSAGE);
                return firstPlayerCanAddItem;
            }
        }

        if (itemFromPlayerRequested is Houses.HouseTransferItem) return SafeTradeError.None;

        var secondPlayerCanAddItem = CanAddItem(playerRequesting, itemFromPlayerRequested, itemFromPlayerRequesting);
        if (secondPlayerCanAddItem is SafeTradeError.None) return secondPlayerCanAddItem;

        // Send an error message to both players if the trade cannot be performed
        OperationFailService.Send(playerRequested.CreatureId, DEFAULT_ERROR_MESSAGE);
        OperationFailService.Send(playerRequesting.CreatureId, DEFAULT_ERROR_MESSAGE);

        return secondPlayerCanAddItem;
    }

    private static SafeTradeError CanAddItem(IPlayer player, IItem item, IItem itemToBeRemoved)
    {
        var inventory = player.Inventory;

        var slotDestination = TradeSlotDestinationQuery.Get(player, item, itemToBeRemoved);

        if (!PlayerHasEnoughCapacity(player, item, itemToBeRemoved))
            return SafeTradeError.PlayerDoesNotHaveEnoughCapacity;

        if (!PlayerHasFreeSlotsToAddTheItem(player, item, itemToBeRemoved, slotDestination))
        {
            OperationFailService.Send(player.CreatureId, "You do not have enough room to carry this object.");
            return SafeTradeError.PlayerDoesNotHaveEnoughRoomToCarry;
        }

        if (itemToBeRemoved == inventory[slotDestination])
            //player has space to allocate the new item
            return SafeTradeError.None;

        return SafeTradeError.None;
    }

    private static bool PlayerHasFreeSlotsToAddTheItem(IPlayer player, IItem itemToAdd, IItem itemToBeRemoved,
        Slot slotDestination)
    {
        var inventory = player.Inventory;

        var backpackSlotIsFree = itemToBeRemoved == inventory.BackpackSlot || inventory.BackpackSlot is null;
        var itemToAddIsBackpack = itemToAdd.Metadata.BodyPosition is Slot.Backpack;

        //player is trading his own backpack and slot destination is backpack then cancel the trade
        if (backpackSlotIsFree && slotDestination is Slot.Backpack) return itemToAddIsBackpack;

        var possibleAmountToAdd = player.Inventory.PossibleAmountToAdd(itemToAdd, (byte)slotDestination);

        return possibleAmountToAdd >= itemToAdd.Amount;
    }

    private static bool PlayerHasEnoughCapacity(IPlayer player, IItem item, IItem itemToBeRemoved)
    {
        var itemToBeRemovedWeight = itemToBeRemoved.Owner is IPlayer ? itemToBeRemoved.Weight : 0;

        var capacityAfterItemRemoval = player.TotalCapacity - player.Inventory.TotalWeight + itemToBeRemovedWeight;

        if (capacityAfterItemRemoval >= item.Weight) return true;

        OperationFailService.Send(player.CreatureId,
            $"You do not have enough capacity to carry this object.\nIt weighs {item.Weight} oz.");
        return false;
    }
}