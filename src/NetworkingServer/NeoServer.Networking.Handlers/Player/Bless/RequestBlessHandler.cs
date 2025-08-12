using NeoServer.Domain.Common.Helpers;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player.Bless;

public class RequestBlessHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;

    public RequestBlessHandler(IGameServer game, ILogger logger)
    {
        _game = game;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (Guard.AnyNull(message, connection)) return;
        
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        _logger.Debug("Player {PlayerName} requested blessing information", player.Name);

        // TODO: Implement blessing request logic
        // This would typically:
        // 1. Check current blessings status
        // 2. Calculate blessing costs based on player level
        // 3. Send blessing dialog/window to client
        // For now, we just log the request to prevent "packet not handled" error

        _logger.Information("Blessing request from player {PlayerName} - implementation pending", player.Name);
    }
}
