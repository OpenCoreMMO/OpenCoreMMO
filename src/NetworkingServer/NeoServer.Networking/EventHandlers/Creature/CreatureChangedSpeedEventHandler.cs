using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureChangedSpeedEventHandler(IMap map, IGameServer game)
    : INetworkingEventHandler<CreatureChangedSpeedEvent>
{
    public void Handle(CreatureChangedSpeedEvent @event)
    {
        if (@event is null) return;

        foreach (var spectator in map.GetPlayersAtPositionZone(@event.Creature.Location))
        {
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            connection.OutgoingPackets.Enqueue(new CreatureChangeSpeedPacket(@event.Creature.CreatureId, @event.Speed));
            connection.Send();
        }
    }
}
