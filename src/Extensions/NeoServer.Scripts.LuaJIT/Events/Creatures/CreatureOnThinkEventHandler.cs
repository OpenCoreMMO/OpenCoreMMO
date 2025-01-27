using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnThinkEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;
    private readonly INpcs _npcs;
    private readonly ILogger _logger;

    public CreatureOnThinkEventHandler(
        ICreatureEvents creatureEvents,
        INpcs npcs,
        ILogger logger)
    {
        _creatureEvents = creatureEvents;
        _npcs = npcs;
        _logger = logger;
    }

    public void Execute(ICreature creature, int interval)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(creature.CreatureId, CreatureEventType.CREATURE_EVENT_THINK))
            creatureEvent.ExecuteOnThink(creature, interval);

        if (creature is INpc npc)
        {
            var npcEvent = _npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_THINK, out var onThinkEvent) ||
                !onThinkEvent.HasValue)
                return;

            // onThink(self, interval)
            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, creature, _logger);
            if (callback.StartScriptInterface(onThinkEvent.Value))
            {
                callback.PushSpecificCreature(creature);
                callback.PushNumber(interval);

                if (callback.PersistLuaState())
                    return;
            }
        }
    }
}