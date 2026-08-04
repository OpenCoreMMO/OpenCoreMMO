using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class CreatureOnAppearEventHandler(
    INpcs npcs,
    ILogger logger)
    : IApplicationEventHandler<CreatureAppearEvent>
{
    public void Handle(CreatureAppearEvent @event)
    {
        if (@event is null) return;

        var self = @event.Self;
        var creature = @event.Creature;

        if (self is INpc npc)
        {
            var npcEvent = npcs.GetEvents(npc.Name);

            if (npcEvent == null ||
                npcEvent.Events == null ||
                npcEvent.Events.Count == 0 ||
                !npcEvent.Events.TryGetValue(NpcEventType.NPCS_EVENT_APPEAR, out var onAppearEvent) ||
                !onAppearEvent.HasValue)
                return;

            var callback = new CreatureCallback(npcEvent.LuaScriptInterface, self, logger);
            if (callback.StartScriptInterface(onAppearEvent.Value))
            {
                callback.PushSpecificCreature(self);
                callback.PushCreature(creature);

                if (callback.PersistLuaState())
                    return;
            }
        }
    }
}
