using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class NpcOnHearEventHandler(INpcs npcs, ILogger logger) : IGameEventHandler
{
    public void Execute(ICreature from, ISociableCreature receiver, SpeechType speechType, string message)
    {
        var npcEvent = npcs.GetEvents(receiver.Name);
        if (npcEvent == null ||
            npcEvent.Events == null ||
            npcEvent.Events.Count == 0 ||
            !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_SAY, out var onSayEvent) ||
            !onSayEvent.HasValue)
            return;

        // onCreatureSay(self, creature, type, message)
        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, receiver, logger);
        if (callback.StartScriptInterface(onSayEvent.Value))
        {
            callback.PushSpecificCreature(receiver as INpc);
            callback.PushCreature(from);
            callback.PushNumber((int)speechType);
            callback.PushString(message);
        }

        if (callback.PersistLuaState())
            return;
    }
}