using System;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

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

        // Send guild MOTD if this is a guild channel - use same pattern as "You've already joined this chat channel"
        if (channel is GuildChatChannel && player.Guild != null && !string.IsNullOrWhiteSpace(player.Guild.Motd))
        {
            // Use TextMessagePacket with MESSAGE_STATUS_DEFAULT (same as the "already joined" message)
            connection.OutgoingPackets.Enqueue(new TextMessagePacket(player.Guild.Motd,
                TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));
            Console.WriteLine($"DEBUG: Sent MOTD as status message for {player.Name}: {player.Guild.Motd}");
        }

        connection.Send();
    }
}