using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerSentMessageEventHandler(IGameServer game) : INetworkingEventHandler<PlayerSentMessageEvent>
{
    public void Handle(PlayerSentMessageEvent @event)
    {
        if (@event is null) return;
        if (string.IsNullOrWhiteSpace(@event.Message) || @event.To is null || @event.From is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.To.CreatureId, out var receiverConnection)) return;

        receiverConnection.OutgoingPackets.Enqueue(new PlayerSendPrivateMessagePacket(@event.From, @event.SpeechType, @event.Message));
        receiverConnection.Send();
    }
}
