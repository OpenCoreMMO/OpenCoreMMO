using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureInjuredEventHandler(ICreatureEvents creatureEvents)
    : IApplicationEventHandler<CreatureInjuredEvent>
{
    public void Handle(CreatureInjuredEvent @event)
    {
        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(
                     @event.Victim.CreatureId,
                     @event.DamageList.RegularDamage.Type == DamageType.ManaDrain
                         ? CreatureEventType.CREATURE_EVENT_MANACHANGE
                         : CreatureEventType.CREATURE_EVENT_HEALTHCHANGE))
            creatureEvent.ExecuteOnDamageReceivedChange(@event.Victim, @event.Enemy as ICreature, @event.DamageList);
    }
}