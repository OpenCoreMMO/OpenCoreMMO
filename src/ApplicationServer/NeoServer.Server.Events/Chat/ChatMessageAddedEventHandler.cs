using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Chat;

public class ChatMessageAddedEventHandler
{
    private readonly IGameServer game;

    public ChatMessageAddedEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(ISociableCreature player, IChatChannel chatChannel, SpeechType speechType, string message)
    {
        if (chatChannel is null) return;
        if (string.IsNullOrWhiteSpace(message)) return;

        foreach (var user in chatChannel.Users)
        {
            if (!game.CreatureManager.GetPlayerConnection(user.Player.CreatureId, out var connection)) continue;
            connection.OutgoingPackets.Enqueue(new MessageToChannelPacket(player, speechType, message,
                chatChannel.Id));
            connection.Send();
        }
    }
}