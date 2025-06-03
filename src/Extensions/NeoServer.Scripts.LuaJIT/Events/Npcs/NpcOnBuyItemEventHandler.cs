using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class NpcOnBuyItemEventHandler(INpcs npcs, ILogger logger) : IGameEventHandler
{
    public void Execute(INpc npc, IPlayer player, IItemType itemType, int amount, uint totalCost, bool inBackpacks,
        bool ignore)
    {
        var npcEvent = npcs.GetEvents(npc.Name);
        if (npcEvent == null ||
            npcEvent.Events == null ||
            npcEvent.Events.Count == 0 ||
            !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_PLAYER_BUY, out var onBuyItemEvent) ||
            !onBuyItemEvent.HasValue)
            return;

        // npc:onBuyItem(player, itemId, subType, amount, ignore, inBackpacks, totalCost)
        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, logger);
        if (callback.StartScriptInterface(onBuyItemEvent.Value))
        {
            callback.PushSpecificCreature(npc);
            callback.PushCreature(player);
            callback.PushNumber(itemType.ServerId);
            callback.PushNumber((int)itemType.Group);
            callback.PushNumber(amount);
            callback.PushBoolean(ignore);
            callback.PushBoolean(inBackpacks);
            callback.PushNumber((int)totalCost);
        }

        if (callback.PersistLuaState())
            return;
    }
}