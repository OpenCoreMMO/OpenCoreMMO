using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.SafeTrade.Operations;
using NeoServer.Domain.SafeTrade.Request;
using NeoServer.Domain.SafeTrade.Trackers;
using NeoServer.Domain.SafeTrade.Validations;

namespace NeoServer.Domain.SafeTrade;

/// <summary>
///     Provides functionality for trading items between two players.
/// </summary>
public class SafeTradeSystem : ITradeService
{
    private readonly TradeItemExchanger _tradeItemExchanger;
    private readonly TradeRequestValidation _tradeRequestValidation;

    public SafeTradeSystem(TradeItemExchanger tradeItemExchanger, IMap map)
    {
        _tradeItemExchanger = tradeItemExchanger;
        _tradeRequestValidation = new TradeRequestValidation(map);

        TradeRequestEventHandler.Init(Cancel);
    }

    /// <summary>
    ///     Cancels and closes a trade request
    /// </summary>
    /// <param name="playerCanceling">The trade request to cancel.</param>
    public void Cancel(IPlayer playerCanceling)
    {
        var tradeRequest = TradeRequestTracker.GetTradeRequest(playerCanceling);

        Cancel(tradeRequest);
    }

    /// <summary>
    ///     Requests a trade between two players.
    /// </summary>
    /// <param name="player">The player requesting the trade.</param>
    /// <param name="secondPlayer">The player being asked to trade.</param>
    /// <param name="item">The item being offered for trade.</param>
    public SafeTradeError Request(IPlayer player, IPlayer secondPlayer, IItem item)
    {
        if (Guard.AnyNull(player, secondPlayer, item)) return SafeTradeError.InvalidParameters;

        var items = GetItems(item);

        var result = _tradeRequestValidation.IsValid(player, secondPlayer, items);

        if (result is not SafeTradeError.None) return result;

        // Create a new trade request object.
        var tradeRequest = new TradeRequest(player, secondPlayer, items);

        // Track trade request
        TradeRequestTracker.Track(tradeRequest);

        //Track all items in the trade
        ItemTradedTracker.TrackItems(items, tradeRequest);

        // Subscribe both players to the trade request event.
        TradeRequestEventHandler.Subscribe(player, items);

        OnTradeRequest?.Invoke(tradeRequest);

        return SafeTradeError.None;
    }

    public void Cancel(TradeRequest tradeRequest)
    {
        if (tradeRequest is null) return;

        var playerRequested = tradeRequest.PlayerRequested;
        var playerRequesting = tradeRequest.PlayerRequesting;

        Close(tradeRequest);

        const string message = "Trade is cancelled.";
        OperationFailService.Send(playerRequested.CreatureId, message);
        OperationFailService.Send(playerRequesting.CreatureId, message);
    }

    /// <summary>
    ///     Accepts a trade request and initiates the item exchange.
    /// </summary>
    /// <param name="player">The player accepting the trade request.</param>
    public SafeTradeError AcceptTrade(IPlayer player)
    {
        var tradeRequest = TradeRequestTracker.GetTradeRequest(player);

        if (tradeRequest is null) return SafeTradeError.None;

        tradeRequest.Accept();

        var playerRequested = tradeRequest.PlayerRequested;
        var lastTradeRequest = TradeRequestTracker.GetTradeRequest(playerRequested);

        if (lastTradeRequest is null || !lastTradeRequest.Accepted) return SafeTradeError.None;

        var exchangeResult = _tradeItemExchanger.Exchange(tradeRequest);

        // Close the trade request even if trade fails
        Close(tradeRequest);

        if (exchangeResult is not SafeTradeError.None) OnTradeAccepted?.Invoke(tradeRequest);

        return exchangeResult;
    }

    /// <summary>
    ///     Closes a trade request and cleans up any event subscriptions.
    /// </summary>
    /// <param name="tradeRequest">The trade request to close.</param>
    private void Close(TradeRequest tradeRequest)
    {
        if (Guard.IsNull(tradeRequest)) return;

        var tradeFromPlayerRequested = TradeRequestTracker.GetTradeRequest(tradeRequest.PlayerRequested);

        // Unsubscribe both players from the trade request event.
        var itemFromPlayerRequesting = tradeRequest.Items;
        var itemFromPlayerRequested = tradeFromPlayerRequested?.Items ?? [];

        TradeRequestEventHandler.Unsubscribe(tradeRequest.PlayerRequesting, itemFromPlayerRequesting);
        TradeRequestEventHandler.Unsubscribe(tradeRequest.PlayerRequested, itemFromPlayerRequested);

        ItemTradedTracker.UntrackItems(itemFromPlayerRequesting.Concat(itemFromPlayerRequested));

        // Reset the LastTradeRequest property for both players.
        TradeRequestTracker.Untrack(tradeRequest.PlayerRequesting);
        TradeRequestTracker.Untrack(tradeRequest.PlayerRequested);

        OnClosed?.Invoke(tradeRequest);
    }

    private static IItem[] GetItems(IItem item)
    {
        var items = new List<IItem> { item };
        if (item is IContainer container) items.AddRange(container.RecursiveItems);
        return items.ToArray();
    }

    public static IItem[] GetTradedItems(IPlayer player)
    {
        return TradeRequestTracker.GetTradeRequest(player)?.Items ?? [];
    }

    #region Events

    public event CloseTrade OnClosed;
    public event RequestTrade OnTradeRequest;
    public event TradeAccept OnTradeAccepted;

    #endregion
}

public delegate void CloseTrade(TradeRequest tradeRequest);

public delegate void RequestTrade(TradeRequest tradeRequest);

public delegate void TradeAccept(TradeRequest tradeRequest);