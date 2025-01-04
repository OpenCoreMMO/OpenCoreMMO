using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnKillEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnKillEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICombatActor creature, ICreature target, bool lastHit)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(creature.CreatureId, CreatureEventType.CREATURE_EVENT_KILL))
            creatureEvent.ExecuteOnKill(creature, target, lastHit);
    }
}