using NeoServer.Networking.Packets.Incoming.Trade;
using NeoServer.Server.Commands.Trade;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Trade;

public class TradeRequestHandler : PacketHandler
{
    private readonly IGameCreatureManager _creatureManager;
    private readonly IDispatcher _dispatcher;
    private readonly TradeRequestCommand _tradeRequestCommand;

    public TradeRequestHandler(TradeRequestCommand tradeRequestCommand, IGameCreatureManager creatureManager,
        IDispatcher dispatcher)
    {
        _tradeRequestCommand = tradeRequestCommand;
        _creatureManager = creatureManager;
        _dispatcher = dispatcher;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var tradeRequestPacket = new TradeRequestPacket(message);
        if (!_creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        if (player is null) return;

        _dispatcher.AddEvent(new Event(2000,
            () => _tradeRequestCommand.RequestTrade(player, tradeRequestPacket)));
    }
}