using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.SafeTrade.Trackers;

namespace NeoServer.Domain.SafeTrade.Request;

/// <summary>
///     Represents a trade request between two players in the game.
/// </summary>
public class TradeRequest
{
    public TradeRequest(IPlayer playerRequesting, IPlayer playerRequested, IItem[] items)
    {
        // Stores the players and item involved in the trade request
        PlayerRequesting = (Player)playerRequesting;
        PlayerRequested = (Player)playerRequested;
        Items = items;
    }

    /// <summary>
    ///     The player initiating the trade.
    /// </summary>
    public Player PlayerRequesting { get; }

    /// <summary>
    ///     The player being requested to trade.
    /// </summary>
    public Player PlayerRequested { get; }

    /// <summary>
    ///     Whether the trade request has been accepted.
    /// </summary>
    public bool Accepted { get; private set; }

    /// <summary>
    ///     Whether the trade request has been accepted.
    /// </summary>
    public IItem[] Items { get; }

    /// <summary>
    ///     Whether the player being requested to trade has acknowledged the trade request.
    /// </summary>
    public bool PlayerAcknowledgedTrade =>
        TradeRequestTracker.GetTradeRequest(PlayerRequested) is { } tradeRequest &&
        tradeRequest.PlayerRequested == PlayerRequesting;

    /// <summary>
    ///     Accepts the trade request.
    /// </summary>
    public void Accept()
    {
        Accepted = true;
    }
}