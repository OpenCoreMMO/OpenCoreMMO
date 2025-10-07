using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Creature;
using NeoServer.Networking.Packets.Outgoing.Item;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureChangedVisibilityEventHandler(IMap map, IGameServer game)
    : INetworkingEventHandler<CreatureChangedVisibilityEvent>
{
    public void Handle(CreatureChangedVisibilityEvent @event)
    {
        var creature = @event.Creature;
        
        foreach (var spectator in map.GetPlayersAtPositionZone(creature.Location))
        {
            if (ReferenceEquals(spectator, creature)) continue;

            if (!game.CreatureManager.GetPlayerConnection(spectator.CreatureId, out var connection)) continue;

            if (!creature.Tile.TryGetStackPositionOfThing((IPlayer)spectator, creature, out var stackPosition))
                continue;

            if (!spectator.CanSee(creature.Location)) continue;

            if (creature.IsInvisible)
            {
                connection.OutgoingPackets.Enqueue(new RemoveTileThingPacket(creature.Tile, stackPosition));
            }
            else
            {
                connection.OutgoingPackets.Enqueue(new AddAtStackPositionPacket(creature, stackPosition));
                connection.OutgoingPackets.Enqueue(new AddCreaturePacket((IPlayer)spectator, creature as IWalkableCreature));
            }

            connection.Send();
        }
    }
}