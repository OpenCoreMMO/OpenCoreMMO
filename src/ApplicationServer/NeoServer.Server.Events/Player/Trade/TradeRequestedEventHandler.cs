using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.SafeTrade.Request;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Trade;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Configurations;

namespace NeoServer.Server.Events.Player.Trade;

public class TradeRequestedEventHandler : IEventHandler
{
    private readonly ClientConfiguration _clientConfiguration;
    private readonly IGameServer _gameServer;

    public TradeRequestedEventHandler(IGameServer gameServer, ClientConfiguration clientConfiguration)
    {
        _gameServer = gameServer;
        _clientConfiguration = clientConfiguration;
    }

    public void Execute(TradeRequest tradeRequest)
    {
        if (Guard.AnyNull(tradeRequest, tradeRequest.PlayerRequesting, tradeRequest.PlayerRequested)) return;

        _gameServer.CreatureManager.GetPlayerConnection(tradeRequest.PlayerRequesting.CreatureId,
            out var playerRequestingConnection);
        _gameServer.CreatureManager.GetPlayerConnection(tradeRequest.PlayerRequested.CreatureId,
            out var playerRequestedConnection);

        EnqueueOwnTrade(tradeRequest, playerRequestingConnection);
        EnqueueCounterTrade(tradeRequest, playerRequestedConnection);
        SendTradeMessage(tradeRequest, playerRequestedConnection);

        playerRequestingConnection?.Send();
        playerRequestedConnection?.Send();
    }

    private void EnqueueOwnTrade(TradeRequest tradeRequest, IConnection connection)
    {
        if (connection?.OutgoingPackets is null)
        {
            return;
        }

        connection.OutgoingPackets.Enqueue(new TradeRequestPacket(tradeRequest.PlayerRequesting.Name,
            tradeRequest.Items)
        {
            ShowItemDescription = ShowItemDescription(connection)
        });
    }

    private void EnqueueCounterTrade(TradeRequest tradeRequest, IConnection connection)
    {
        if (connection?.OutgoingPackets is null)
        {
            return;
        }

        connection.OutgoingPackets.Enqueue(new TradeRequestPacket(tradeRequest.PlayerRequesting.Name,
            tradeRequest.Items, acknowledged: true)
        {
            ShowItemDescription = ShowItemDescription(connection)
        });
    }

    private static void SendTradeMessage(TradeRequest tradeRequest, IConnection playerRequestedConnection)
    {
        if (tradeRequest.PlayerAcknowledgedTrade)
        {
            return;
        }

        if (playerRequestedConnection?.OutgoingPackets is null)
        {
            return;
        }

        var message = $"{tradeRequest.PlayerRequesting.Name} wants to trade with you.";
        playerRequestedConnection.OutgoingPackets.Enqueue(new TextMessagePacket(message,
            TextMessageOutgoingType.Small));
    }

    private bool ShowItemDescription(IConnection connection)
    {
        return connection.OtcV8Version > 0 && _clientConfiguration.OtcV8 is { GameItemTooltip: true };
    }
}
