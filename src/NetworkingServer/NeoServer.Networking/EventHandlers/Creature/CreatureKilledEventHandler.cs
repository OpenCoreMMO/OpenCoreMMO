using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Networking.Packets.Outgoing.Login;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.EventHandlers.Creature;

public class CreatureKilledEventHandler(IGameServer game, IScriptGameManager scriptGameManager) : INetworkEventHandler<ICreature>
{
    public void Execute(ICombatActor creature, IThing by)
    {
        game.Scheduler.AddEvent(new SchedulerEvent(200, () =>
        {
            //send packets to killed player
            if (creature is not IPlayer  ||
                !game.CreatureManager.GetPlayerConnection(creature.CreatureId, out var connection)) return;
            
            connection.OutgoingPackets.Enqueue(new ReLoginWindowOutgoingPacket());
            connection.Send();

            scriptGameManager.CreatureEventExecuteOnCreatureDeath(creature, by);
        }));
    }

    public void Subscribe(ICreature entity)
    {
        if (entity is not ICombatActor combatActor) return;
        combatActor.OnDeath += Execute;
    }
    public void Unsubscribe(ICreature creature)
    {
        if (creature is not ICombatActor combatActor) return;
        combatActor.OnDeath -= Execute;
    }
}