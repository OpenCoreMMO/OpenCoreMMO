using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Creatures;

namespace NeoServer.Scripts.LuaJIT;

public class CreatureEventSubscriber : ICreatureEventSubscriber, IGameEventSubscriber
{
    private readonly CreatureOnDeathEventHandler creatureKilledEventHandler;

    public CreatureEventSubscriber(
        CreatureOnDeathEventHandler creatureKilledEventHandler)
    {
        this.creatureKilledEventHandler = creatureKilledEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnKilled += creatureKilledEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor actor)
        {
            actor.OnKilled -= creatureKilledEventHandler.Execute;
        }
    }
}