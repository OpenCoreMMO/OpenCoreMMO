using NeoServer.Game.Systems.SafeTrade;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Trade;

public class TradeAcceptHandler : PacketHandler
{
    private readonly IGameCreatureManager _creatureManager;
    private readonly IDispatcher _dispatcher;
    private readonly SafeTradeSystem _tradeSystem;

    public TradeAcceptHandler(SafeTradeSystem tradeSystem, IDispatcher dispatcher, IGameCreatureManager creatureManager)
    {
        _tradeSystem = tradeSystem;
        _dispatcher = dispatcher;
        _creatureManager = creatureManager;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_creatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;
        if (player is null) return;

        _dispatcher.AddEvent(new Event(() => _tradeSystem.AcceptTrade(player)));
    }
}