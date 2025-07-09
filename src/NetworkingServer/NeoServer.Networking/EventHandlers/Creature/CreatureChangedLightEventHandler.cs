using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureChangedLightEventHandler(IMap map, IGameCreatureManager creatureManager)
    : INetworkingEventHandler<CreatureChangedLightEvent>
{
    public void Handle(CreatureChangedLightEvent @event)
    {
        var creature = @event.Creature;

        foreach (var spectator in map.GetPlayersAtPositionZone(creature.Location))
        {
            if (!creature.CanSee(spectator.Location)) continue;

            if (!creatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            connection.OutgoingPackets.Enqueue(new CreatureLightPacket(creature as IPlayer));
            connection.Send();
        }
    }
}