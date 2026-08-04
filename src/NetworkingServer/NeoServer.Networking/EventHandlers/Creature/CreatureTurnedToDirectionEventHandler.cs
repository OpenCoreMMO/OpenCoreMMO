using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureTurnedToDirectionEventHandler(IMap map, IGameServer game)
    : INetworkingEventHandler<CreatureTurnedToDirectionEvent>
{
    public void Handle(CreatureTurnedToDirectionEvent @event)
    {
        if (@event is null) return;

        var creature = @event.Creature;
        var direction = @event.Direction;

        if (creature.IsInvisible) return;

        foreach (var spectator in map.GetSpectators(creature.Location, true))
        {
            if (!spectator.CanSee(creature.Location)) continue;
            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;
            if (!game.CreatureManager.TryGetPlayer(spectator.CreatureId, out var player)) continue;
            if (!creature.Tile.TryGetStackPositionOfThing(player, creature, out var stackPosition)) continue;

            connection.OutgoingPackets.Enqueue(new TurnToDirectionPacket(creature, direction, stackPosition));
            connection.Send();
        }
    }
}
