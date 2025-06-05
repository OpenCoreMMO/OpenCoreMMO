using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player.Containers;

public class PlayerClosedContainerEventHandler
{
    private readonly IGameServer _game;

    public PlayerClosedContainerEventHandler(IGameServer game)
    {
        _game = game;
    }

    public void Execute(IPlayer player, byte containerId, IContainer container)
    {
        if (!_game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new CloseContainerPacket(containerId));
        connection.Send();
    }
}