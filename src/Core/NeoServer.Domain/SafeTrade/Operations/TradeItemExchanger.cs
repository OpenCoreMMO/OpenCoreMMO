using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Houses;
using NeoServer.Domain.SafeTrade.Request;
using NeoServer.Domain.SafeTrade.Trackers;
using NeoServer.Domain.SafeTrade.Validations;

namespace NeoServer.Domain.SafeTrade.Operations;

/// <summary>
///     A class that allows players to exchange items with each other in a game.
/// </summary>
/// <remarks>
///     The class takes in an IItemRemoveService object in its constructor to remove items from the world when they are
///     exchanged.
///     It contains methods to check if a trade can be performed, add items to a player's inventory, and exchange items
///     between players.
/// </remarks>
public class TradeItemExchanger
{
    private const string DEFAULT_ERROR_MESSAGE = "Trade could not be completed.";
    private readonly IItemRemoveService _itemRemoveService;

    public TradeItemExchanger(IItemRemoveService itemRemoveService)
    {
        _itemRemoveService = itemRemoveService;
    }

    /// <summary>
    ///     Attempts to exchange items between two players.
    /// </summary>
    /// <param name="tradeRequest">The trade request containing the two players and their items to exchange.</param>
    /// <returns>True if the exchange was successful, false otherwise.</returns>
    public SafeTradeError Exchange(TradeRequest tradeRequest)
    {
        var playerRequesting = tradeRequest.PlayerRequesting;
        var playerRequested = tradeRequest.PlayerRequested;

        var secondTradeRequest = TradeRequestTracker.GetTradeRequest(playerRequested);
        if (secondTradeRequest is null) return SafeTradeError.InvalidParameters;

        // Get the last item requested from each player
        var itemFromPlayerRequesting = tradeRequest.Items[0];
        var itemFromPlayerRequested = secondTradeRequest.Items[0];

        var validationResult = TradeExchangeValidation.CanPerformTrade(playerRequested, itemFromPlayerRequesting,
            playerRequesting, itemFromPlayerRequested);

        if (validationResult is not SafeTradeError.None) return validationResult;

        //untrack items before exchanging them
        ItemTradedTracker.UntrackItems(tradeRequest.Items.Concat(secondTradeRequest.Items));

        return ExchangeItem(playerRequesting, playerRequested, itemFromPlayerRequested, itemFromPlayerRequesting);
    }

    private SafeTradeError ExchangeItem(IPlayer playerRequesting, IPlayer playerRequested, IItem itemFromPlayerRequested,
        IItem itemFromPlayerRequesting)
    {
        if (Guard.AnyNull(itemFromPlayerRequested, playerRequesting, itemFromPlayerRequesting, playerRequested))
        {
            return SafeTradeError.InvalidParameters;
        }

        var houseTransferFromRequestingPlayer = itemFromPlayerRequesting as HouseTransferItem;
        var houseTransferFromRequestedPlayer = itemFromPlayerRequested as HouseTransferItem;

        if (houseTransferFromRequestingPlayer is not null && houseTransferFromRequestedPlayer is not null)
        {
            return SafeTradeError.InvalidParameters;
        }

        if (houseTransferFromRequestingPlayer is not null)
        {
            return CompleteHouseTransfer(
                houseTransferFromRequestingPlayer,
                playerRequested,
                playerRequesting,
                itemFromPlayerRequested,
                itemFromPlayerRequesting);
        }

        if (houseTransferFromRequestedPlayer is not null)
        {
            return CompleteHouseTransfer(
                houseTransferFromRequestedPlayer,
                playerRequesting,
                playerRequested,
                itemFromPlayerRequesting,
                itemFromPlayerRequested);
        }

        var playerRequestingSlotDestination =
            TradeSlotDestinationQuery.Get(playerRequesting, itemFromPlayerRequested, itemFromPlayerRequesting);

        var playerRequestedSlotDestination =
            TradeSlotDestinationQuery.Get(playerRequested, itemFromPlayerRequesting, itemFromPlayerRequested);

        _itemRemoveService.Remove(itemFromPlayerRequesting);
        _itemRemoveService.Remove(itemFromPlayerRequested);

        AddItemToInventory(playerRequesting, itemFromPlayerRequested, playerRequestingSlotDestination);
        AddItemToInventory(playerRequested, itemFromPlayerRequesting, playerRequestedSlotDestination);
        return SafeTradeError.None;
    }

    private SafeTradeError CompleteHouseTransfer(
        HouseTransferItem transferItem,
        IPlayer buyer,
        IPlayer seller,
        IItem payment,
        IItem transferDocument)
    {
        if (!transferItem.Complete(buyer))
        {
            OperationFailService.Send(seller.CreatureId, DEFAULT_ERROR_MESSAGE);
            OperationFailService.Send(buyer.CreatureId, DEFAULT_ERROR_MESSAGE);
            return SafeTradeError.InvalidParameters;
        }

        ExchangeOfferedItem(seller, payment, transferDocument);
        return SafeTradeError.None;
    }

    private void ExchangeOfferedItem(IPlayer receiver, IItem offeredItem, IItem transferDocument)
    {
        var slotDestination = TradeSlotDestinationQuery.Get(receiver, offeredItem, transferDocument);
        _itemRemoveService.Remove(offeredItem);
        AddItemToInventory(receiver, offeredItem, slotDestination);
    }

    private static void AddItemToInventory(IPlayer player, IItem item, Slot slot)
    {
        player.Inventory.AddItem(item, slot);
    }
}