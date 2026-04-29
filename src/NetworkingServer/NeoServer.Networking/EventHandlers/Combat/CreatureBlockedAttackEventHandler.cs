using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Combat;

public class CreatureBlockedAttackEventHandler(IGameServer game)
    : INetworkingEventHandler<CreatureBlockedAttackEvent>
{
    public void Handle(CreatureBlockedAttackEvent @event)
    {
        if (@event is null) return;

        foreach (var spectator in game.Map.GetPlayersAtPositionZone(@event.Actor.Location))
        {
            var effect = @event.BlockType == BlockType.Armor ? EffectT.SparkYellow : EffectT.Puff;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(@event.Actor.Location, effect));
            connection.Send();
        }
    }
}
