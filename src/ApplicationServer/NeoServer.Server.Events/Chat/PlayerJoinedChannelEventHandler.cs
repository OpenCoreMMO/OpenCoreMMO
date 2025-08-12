using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Chat;

public class PlayerJoinedChannelEventHandler
{
    private readonly IGameServer game;

    public PlayerJoinedChannelEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player, ChatChannel channel)
    {
        if (channel is null) return;
        if (player is null) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new PlayerOpenChannelPacket(channel.Id, channel.Name));

        //todo: 1098 revise this
        if (!string.IsNullOrWhiteSpace(channel.Description))
            connection.OutgoingPackets.Enqueue(new MessageToChannelPacket(null, SpeechType.ChannelYellow,
                channel.Description, channel.Id));

        connection.Send();
    }
}