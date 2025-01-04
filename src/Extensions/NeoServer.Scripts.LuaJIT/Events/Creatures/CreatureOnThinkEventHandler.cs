using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnThinkEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public CreatureOnThinkEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(ICreature creature, int interval)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(creature.CreatureId, CreatureEventType.CREATURE_EVENT_THINK))
            creatureEvent.ExecuteOnThink(creature, interval);
    }
}