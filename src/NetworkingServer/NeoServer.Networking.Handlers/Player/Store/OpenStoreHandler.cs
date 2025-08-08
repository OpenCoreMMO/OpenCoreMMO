using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player.Store;

public class OpenStoreHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;

    public OpenStoreHandler(IGameServer game, ILogger logger)
    {
        _game = game;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var player = _game.CreatureManager.TryGetPlayer(connection.CreatureId, out var p) ? p : null;
        
        if (player == null)
        {
            _logger.Warning("OpenStore packet received but player not found for connection {CreatureId}", connection.CreatureId);
            return;
        }

        _logger.Debug("OpenStore packet received from player {PlayerName}", player.Name);
        
        // This packet is sent when player requests to open the store
        // For now, we just log it since store system may not be fully implemented
        // In a full implementation, this would open the store window
    }
}
