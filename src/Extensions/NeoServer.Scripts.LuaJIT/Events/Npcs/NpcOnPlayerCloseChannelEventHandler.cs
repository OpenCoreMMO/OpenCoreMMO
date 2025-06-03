using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Npcs;

public class NpcOnPlayerCloseChannelEventHandler(INpcs npcs, ILogger logger) : IGameEventHandler
{
    public void Execute(INpc npc, IPlayer player)
    {
        var npcEvent = npcs.GetEvents(npc.Name);
        if (npcEvent == null ||
            npcEvent.Events == null ||
            npcEvent.Events.Count == 0 ||
            !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_PLAYER_CLOSE_CHANNEL,
                out var onPlayerCloseChannelEvent) ||
            !onPlayerCloseChannelEvent.HasValue)
            return;

        // onPlayerCloseChannel(npc, player)
        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, logger);
        if (callback.StartScriptInterface(onPlayerCloseChannelEvent.Value))
        {
            callback.PushSpecificCreature(npc);
            callback.PushCreature(player);
        }

        if (callback.PersistLuaState())
            return;
    }
}