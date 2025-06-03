using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnManaChangeEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnManaChangeEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor actor, ICreature attacker, CombatDamage damage)
    {
        //todo: implement this
        //foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(actor.CreatureId, CreatureEventType.CREATURE_EVENT_MANACHANGE))
        //    creatureEvent.ExecuteOnManaChange(creature, attacker, damage);
    }
}