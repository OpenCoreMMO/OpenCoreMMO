using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Configurations;

namespace NeoServer.Server.Events.Player.Containers;

public class PlayerOpenedContainerEventHandler
{
    private readonly ClientConfiguration _clientConfiguration;
    private readonly IGameServer game;

    public PlayerOpenedContainerEventHandler(IGameServer game, ClientConfiguration clientConfiguration)
    {
        this.game = game;
        _clientConfiguration = clientConfiguration;
    }

    public void Execute(IPlayer player, byte containerId, IContainer container)
    {
        SendContainerPacket(player, containerId, container);
    }

    private void SendContainerPacket(IPlayer player, byte containerId, IContainer container)
    {
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new OpenContainerPacket(container, containerId)
        {
            WithDescription = connection.OtcV8Version > 0 && _clientConfiguration.OtcV8.GameItemTooltip
        });
        connection.Send();
    }
}