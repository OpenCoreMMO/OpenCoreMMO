using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player.Quest;

public class RequestQuestLogHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;

    public RequestQuestLogHandler(IGameServer game, ILogger logger)
    {
        _game = game;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var player = _game.CreatureManager.TryGetPlayer(connection.CreatureId, out var p) ? p : null;
        
        if (player == null)
        {
            _logger.Warning("RequestQuestLog packet received but player not found for connection {CreatureId}", connection.CreatureId);
            return;
        }

        _logger.Debug("RequestQuestLog packet received from player {PlayerName}", player.Name);
        
        // This packet is sent when player requests to view the quest log
        // The server should respond with the player's quest information
        // For now, we just log it since quest system may not be fully implemented
        // In a full implementation, this would send quest log data to the client
    }
}
