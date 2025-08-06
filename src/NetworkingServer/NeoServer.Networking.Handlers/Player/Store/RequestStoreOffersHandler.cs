using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player.Store;

public class RequestStoreOffersHandler : PacketHandler
{
    private readonly IGameServer _gameServer;
    private readonly ILogger _logger;

    public RequestStoreOffersHandler(IGameServer gameServer, ILogger logger)
    {
        _gameServer = gameServer;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_gameServer.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
        {
            _logger.Warning("RequestStoreOffers handler called but player not found for connection {ConnectionId}", connection.CreatureId);
            return;
        }

        _logger.Debug("Player {PlayerName} requested store offers", player.Name);
        
        // TODO: Implement store offers functionality
        // This would typically send back a list of store items/offers to the client
        // For now, we just log the request to prevent the "not handled" error
    }
}
