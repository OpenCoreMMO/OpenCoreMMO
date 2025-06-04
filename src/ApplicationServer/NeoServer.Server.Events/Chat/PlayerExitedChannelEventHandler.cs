using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Chat;

public class PlayerExitedChannelEventHandler
{
    private readonly IGameServer game;

    public PlayerExitedChannelEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, ChatChannel channel)
    {
        if (channel is null) return;
        if (player is null) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerCloseChannelPacket(channel.Id));
        connection.Send();
    }
}