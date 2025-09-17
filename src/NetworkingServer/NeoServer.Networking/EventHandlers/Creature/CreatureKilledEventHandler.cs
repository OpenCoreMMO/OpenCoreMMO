using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureKilledEventHandler(IGameServer game) : INetworkingEventHandler<CreatureDeathEvent>
{
    public void Handle(CreatureDeathEvent @event)
    {
        ICombatActor creature = @event.DeadCreature;
        IThing by = @event.Attacker;
        
        game.Scheduler.AddEvent(new SchedulerEvent(200, () =>
        {
            //send packets to killed player
            if (creature is not IPlayer ||
                !game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) return;

            connection.OutgoingPackets.Enqueue(new ReLoginWindowOutgoingPacket());
            connection.Send();
        }));
    }
}