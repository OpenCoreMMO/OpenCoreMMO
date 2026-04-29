using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Npcs;

namespace NeoServer.Scripts.LuaJIT.Events;

public class CreatureEventsSubscriber(
    NpcOnPlayerCloseChannelEventHandler npcOnPlayerCloseChannelEventHandler,
    NpcOnSellItemEventHandler npcOnSellItemEventHandler,
    NpcOnBuyItemEventHandler npcOnBuyItemEventHandler) : ICreatureEventSubscriber, IGameEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is INpc npc)
        {
            npc.OnPlayerCloseChannel += npcOnPlayerCloseChannelEventHandler.Execute;
        }

        if (creature is IShopperNpc npcShopper)
        {
            npcShopper.OnSellItem += npcOnSellItemEventHandler.Execute;
            npcShopper.OnBuyItem += npcOnBuyItemEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is INpc npc)
        {
            npc.OnPlayerCloseChannel -= npcOnPlayerCloseChannelEventHandler.Execute;
        }

        if (creature is IShopperNpc npcShopper)
        {
            npcShopper.OnSellItem -= npcOnSellItemEventHandler.Execute;
            npcShopper.OnBuyItem -= npcOnBuyItemEventHandler.Execute;
        }
    }
}
