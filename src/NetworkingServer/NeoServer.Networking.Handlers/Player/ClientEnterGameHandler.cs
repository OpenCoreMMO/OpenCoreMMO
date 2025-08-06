using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player;

public class ClientEnterGameHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;

    public ClientEnterGameHandler(IGameServer game, ILogger logger)
    {
        _game = game;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var player = _game.CreatureManager.TryGetPlayer(connection.CreatureId, out var p) ? p : null;
        
        if (player == null)
        {
            _logger.Warning("ClientEnterGame packet received but player not found for connection {CreatureId}", connection.CreatureId);
            return;
        }

        _logger.Debug("ClientEnterGame packet received from player {PlayerName}", player.Name);
        
        // This packet is typically sent when client enters the game
        // The server should respond with game state information
        // For now, we just log it as it's likely handled elsewhere in the login flow
    }
}
