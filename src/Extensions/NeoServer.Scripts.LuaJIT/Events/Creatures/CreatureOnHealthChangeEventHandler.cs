using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Models.Bases.Events;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnHealthChangeEventHandler(ICreatureEvents creatureEvents)
    : IApplicationEventHandler<CreatureHealthChangedEvent>
{
    public void Handle(CreatureHealthChangedEvent @event)
    {
        //todo: implement this
        //foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(actor.CreatureId, CreatureEventType.CREATURE_EVENT_HEALTHCHANGE))
        //    creatureEvent.ExecuteOnHealthChange(actor, attacker, damage);
    }
}