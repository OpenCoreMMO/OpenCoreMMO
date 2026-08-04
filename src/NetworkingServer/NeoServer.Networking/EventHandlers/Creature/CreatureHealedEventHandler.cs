using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureHealedEventHandler(IMap map, IGameCreatureManager gameCreatureManager)
    : INetworkingEventHandler<CreatureHealedEvent>
{
    public void Handle(CreatureHealedEvent @event)
    {
        var healedCreature = @event.HealedCreature;

        foreach (var spectator in map.GetPlayersAtPositionZone(healedCreature.Location))
        {
            if (!gameCreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (Equals(healedCreature, spectator))
                connection.OutgoingPackets.Enqueue(new PlayerStatusPacket((IPlayer)healedCreature));

            connection.OutgoingPackets.Enqueue(new CreatureHealthPacket(healedCreature));

            connection.Send();
        }
    }
}
