using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnThinkEventHandler(
    ICreatureEvents creatureEvents,
    INpcs npcs,
    ILogger logger)
    : IApplicationEventHandler<CreatureThinkEvent>
{
    public void Handle(CreatureThinkEvent @event)
    {
        if (@event is null) return;

        var creature = @event.Creature;
        var interval = @event.Interval;

        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(creature.CreatureId,
                     CreatureEventType.CREATURE_EVENT_THINK))
            creatureEvent.ExecuteOnThink(creature, interval);

        if (creature is INpc npc)
        {
            var npcEvent = npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcEventType.NPCS_EVENT_THINK, out var onThinkEvent) ||
                !onThinkEvent.HasValue)
                return;

            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, creature, logger);
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
