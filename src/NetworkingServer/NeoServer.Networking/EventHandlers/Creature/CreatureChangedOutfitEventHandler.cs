using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureChangedOutfitEventHandler(IMap map, IGameServer game)
    : INetworkingEventHandler<CreatureChangedOutfitEvent>
{
    public void Handle(CreatureChangedOutfitEvent @event)
    {
        if (@event is null) return;

        foreach (var spectator in map.GetPlayersAtPositionZone(@event.Creature.Location))
        {
            if (!@event.Creature.CanSee(spectator.Location)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            connection.OutgoingPackets.Enqueue(new CreatureOutfitPacket(@event.Creature));
            connection.Send();
        }
    }
}
