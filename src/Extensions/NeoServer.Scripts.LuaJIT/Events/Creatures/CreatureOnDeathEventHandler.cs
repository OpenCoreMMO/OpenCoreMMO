using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnDeathEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<CreatureDeathEvent>
{
    public void Handle(CreatureDeathEvent @event)
    {
        //TODO: we need to move this to the CreatureDeathEventHandler in the Events project
        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(@event.DeadCreature.CreatureId,
                     CreatureEventType.CREATURE_EVENT_DEATH))
            creatureEvent.ExecuteOnDeath(@event.DeadCreature, @event.DeadCreature.Corpse as IItem, @event.Attacker as ICreature, null, false, false);
    }
}