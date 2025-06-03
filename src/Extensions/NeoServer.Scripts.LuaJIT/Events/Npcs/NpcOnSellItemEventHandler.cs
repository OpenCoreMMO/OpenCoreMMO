using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

public class NpcOnSellItemEventHandler(INpcs npcs, ILogger logger) : IGameEventHandler
{
    public void Execute(INpc npc, IPlayer player, IItemType itemType, int amount, uint totalCost, bool ignore)
    {
        var npcEvent = npcs.GetEvents(npc.Name);
        if (npcEvent == null ||
            npcEvent.Events == null ||
            npcEvent.Events.Count == 0 ||
            !npcEvent.Events.TryGetValue(NpcsEventType.NPCS_EVENT_PLAYER_SELL, out var onSellItemEvent) ||
            !onSellItemEvent.HasValue)
            return;

        // npc:onSellItem(player, itemId, subType, amount, ignore, itemName, totalCost)
        var callback = new CreatureCallback(npcEvent.LuaScriptInterface, npc, logger);
        if (callback.StartScriptInterface(onSellItemEvent.Value))
        {
            callback.PushSpecificCreature(npc);
            callback.PushCreature(player);
            callback.PushNumber(itemType.ServerId);
            callback.PushNumber((int)itemType.Group);
            callback.PushNumber(amount);
            callback.PushBoolean(ignore);
            callback.PushString(itemType.Name);
            callback.PushNumber((int)totalCost);
        }

        if (callback.PersistLuaState())
            return;
    }
}