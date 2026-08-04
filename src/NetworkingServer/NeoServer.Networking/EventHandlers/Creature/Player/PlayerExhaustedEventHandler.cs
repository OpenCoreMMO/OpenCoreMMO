using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature.Player;

public class PlayerExhaustedEventHandler(IGameServer game) : INetworkingEventHandler<PlayerExhaustedEvent>
{
    public void Handle(PlayerExhaustedEvent @event)
    {
        if (@event is null) return;

        if (!game.CreatureManager.GetPlayerConnection(@event.Player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Player.Location, EffectT.Puff));
        connection.OutgoingPackets.Enqueue(new TextMessagePacket(TextConstants.YOU_ARE_EXHAUSTED,
            TextMessageOutgoingType.Small));

        connection.Send();
    }
}
