using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureHearEventHandler(IGameServer game) : INetworkingEventHandler<CreatureHearEvent>
{
    public void Handle(CreatureHearEvent @event)
    {
        if (@event is null) return;
        if (@event.From is null || @event.Receiver is null || @event.SpeechType == SpeechType.None || string.IsNullOrEmpty(@event.Message)) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Receiver.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new CreatureSayPacket(@event.From, @event.SpeechType, @event.Message));
        connection.Send();
    }
}
