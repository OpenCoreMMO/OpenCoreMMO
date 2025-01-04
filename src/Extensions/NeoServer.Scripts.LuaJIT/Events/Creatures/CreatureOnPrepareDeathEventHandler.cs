using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnPrepareDeathEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnPrepareDeathEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor actor, ICombatActor killer, int realDamage)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(actor.CreatureId, CreatureEventType.CREATURE_EVENT_PREPAREDEATH))
            creatureEvent.ExecuteOnPrepareDeath(actor, killer, realDamage);
    }
}