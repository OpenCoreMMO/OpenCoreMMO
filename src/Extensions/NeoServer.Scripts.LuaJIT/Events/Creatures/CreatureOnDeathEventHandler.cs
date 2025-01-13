using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnDeathEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnDeathEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor actor, IThing by)
    {
        //TODO: we need to move this to the CreatureDeathEventHandler in the Events project
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(actor.CreatureId, CreatureEventType.CREATURE_EVENT_DEATH))
            creatureEvent.ExecuteOnDeath(actor, actor.Corpse as IItem, by as ICreature, null, false, false);
    }
}