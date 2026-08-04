using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnPrepareDeathEventHandler(ICreatureEvents creatureEvents)
    : IApplicationEventHandler<CreatureBeforeDeathEvent>
{
    public void Handle(CreatureBeforeDeathEvent @event)
    {
        if (@event is null) return;

        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(@event.Creature.CreatureId,
                     CreatureEventType.CREATURE_EVENT_PREPAREDEATH))
            creatureEvent.ExecuteOnPrepareDeath(@event.Creature, @event.Killer, @event.RealDamage);
    }
}
