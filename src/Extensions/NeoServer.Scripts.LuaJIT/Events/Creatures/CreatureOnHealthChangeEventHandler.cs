using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnHealthChangeEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnHealthChangeEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor actor, ICreature attacker, CombatDamage damage)
    {
        //todo: implement this
        //foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(actor.CreatureId, CreatureEventType.CREATURE_EVENT_HEALTHCHANGE))
        //    creatureEvent.ExecuteOnHealthChange(actor, attacker, damage);
    }
}