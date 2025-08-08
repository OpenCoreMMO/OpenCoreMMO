using NeoServer.Domain.Common.Helpers;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using Serilog;

namespace NeoServer.Networking.Handlers.Player.DailyReward;

public class PlayerDailyRewardHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly ILogger _logger;

    public PlayerDailyRewardHandler(IGameServer game, ILogger logger)
    {
        _game = game;
        _logger = logger;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (Guard.AnyNull(message, connection)) return;
        
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        // Read packet data
        var bonusShrine = message.GetByte();
        var itemCount = message.GetByte();
        
        // TODO: Implement daily reward logic
        // For now, just log the request
        _logger.Information("Player {PlayerName} requested daily reward with bonusShrine: {BonusShrine}, itemCount: {ItemCount}", 
            player.Name, bonusShrine, itemCount);
            
        // Read the items data (even if we don't use it yet)
        for (var i = 0; i < itemCount; i++)
        {
            var itemId = message.GetUInt16();
            var count = message.GetByte();
            _logger.Debug("Daily reward item: {ItemId}, count: {Count}", itemId, count);
        }
    }
}
