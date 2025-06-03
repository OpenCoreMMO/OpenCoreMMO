using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class PlayerCannotUseSpellEventHandler(IGameServer game) : INetworkingEventHandler<SpellFailedToCastEvent>
{
    public void Handle(SpellFailedToCastEvent @event)
    {
        foreach (var spectator in game.Map.GetPlayersAtPositionZone(@event.Caster.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Caster.Location, EffectT.Puff));
            connection.OutgoingPackets.Enqueue(new TextMessagePacket(TextMessageOutgoingParser.Parse(@event.Error),
                TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));
            connection.Send();
        }
    }
}