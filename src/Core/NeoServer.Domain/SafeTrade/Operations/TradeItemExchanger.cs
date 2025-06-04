using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Player.Inventory;
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

        ExchangeItem(playerRequesting, playerRequested, itemFromPlayerRequested, itemFromPlayerRequesting);

        return SafeTradeError.None;
    }

    private void ExchangeItem(IPlayer playerRequesting, IPlayer playerRequested, IItem itemFromPlayerRequested,
        IItem itemFromPlayerRequesting)
    {
        if (Guard.AnyNull(itemFromPlayerRequested, playerRequesting, itemFromPlayerRequesting, playerRequested)) return;

        var playerRequestingSlotDestination =
            TradeSlotDestinationQuery.Get(playerRequesting, itemFromPlayerRequested, itemFromPlayerRequesting);

        var playerRequestedSlotDestination =
            TradeSlotDestinationQuery.Get(playerRequested, itemFromPlayerRequesting, itemFromPlayerRequested);

        // Remove the items from their previous locations
        _itemRemoveService.Remove(itemFromPlayerRequesting);
        _itemRemoveService.Remove(itemFromPlayerRequested);

        // Add the items to each player's inventory
        AddItemToInventory(playerRequesting, itemFromPlayerRequested, playerRequestingSlotDestination);
        AddItemToInventory(playerRequested, itemFromPlayerRequesting, playerRequestedSlotDestination);
    }

    private static void AddItemToInventory(IPlayer player, IItem item, Slot slot)
    {
        player.Inventory.AddItem(item, slot);
    }
}