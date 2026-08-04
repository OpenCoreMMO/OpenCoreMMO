using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player.Movement;

public class PlayerAutoWalkHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerAutoWalkHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var autoWalk = new AutoWalkPacket(message);

        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
            _game.Dispatcher.AddEvent(new Event(() =>
            {
                player.StopFollowing();
                player.WalkTo(autoWalk.Steps);
            }));
    }
}