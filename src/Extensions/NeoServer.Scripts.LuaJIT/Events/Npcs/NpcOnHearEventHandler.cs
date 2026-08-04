using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Npcs;

public class NpcOnHearEventHandler(INpcs npcs, ILogger logger) : IApplicationEventHandler<CreatureHearEvent>
{
    public void Handle(CreatureHearEvent @event)
    {
        if (@event is null) return;

        if (@event.Receiver is not INpc npc) return;

        var npcEvent = npcs.GetEvents(npc.Name);
        if (npcEvent?.Events is null ||
            npcEvent.Events.Count == 0 ||
            !npcEvent.Events.TryGetValue(NpcEventType.NPCS_EVENT_SAY, out var onSayEvent) ||
            !onSayEvent.HasValue)
            return;

        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, logger);
        if (callback.StartScriptInterface(onSayEvent.Value))
        {
            callback.PushSpecificCreature(npc);
            callback.PushCreature(@event.From);
            callback.PushNumber((int)@event.SpeechType);
            callback.PushString(@event.Message);
        }

        if (callback.PersistLuaState())
            return;
    }
}