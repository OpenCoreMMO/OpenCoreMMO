using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player;

public class PlayerLookAtHandler : PacketHandler
{
    private readonly IGameServer _game;

    public PlayerLookAtHandler(IGameServer game)
    {
        _game = game;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var lookAtPacket = new LookAtPacket(message);

        if (_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player))
        {
            if (lookAtPacket.Location.Type == LocationType.Ground)
            {
                if (_game.Map[lookAtPacket.Location] is not ITile tile) return;

                _game.Dispatcher.AddEvent(new Event(() => player.LookAt(tile)));
            }

            if (lookAtPacket.Location.Type == LocationType.Container)
                _game.Dispatcher.AddEvent(new Event(() =>
                    player.LookAt(lookAtPacket.Location.ContainerId, lookAtPacket.Location.ContainerSlot)));

            if (lookAtPacket.Location.Type == LocationType.Slot)
                _game.Dispatcher.AddEvent(new Event(() => player.LookAt(lookAtPacket.Location.Slot)));
        }
    }
}