using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerCloseNpcChannelHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerCloseNpcChannelHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        foreach (var creature in _game.Map.GetCreaturesAtPositionZone(player.Location))
            if (creature is INpc npc)
                _game.Dispatcher.AddEvent(new Event(() => npc.PlayerCloseChannel(player)));
    }
}