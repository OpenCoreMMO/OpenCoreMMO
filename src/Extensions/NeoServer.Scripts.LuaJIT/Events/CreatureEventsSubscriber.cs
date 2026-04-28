using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Creatures;
using NeoServer.Scripts.LuaJIT.Events.Npcs;
using NeoServer.Scripts.LuaJIT.Events.Players;

namespace NeoServer.Scripts.LuaJIT.Events;

public class CreatureEventsSubscriber(
    CreatureOnThinkEventHandler creatureOnThinkEventHandler,
    CreatureOnPrepareDeathEventHandler creatureOnPrepareDeathEventHandler,
    PlayerOnTextEditEventHandler playerOnTextEditEventHandler,
    CreatureOnAppearEventHandler creatureOnAppearEventHandler,
    CreatureOnDisappearEventHandler creatureOnDisappearEventHandler,
    CreatureOnMoveEventHandler creatureOnMoveEventHandler,
    NpcOnPlayerCloseChannelEventHandler npcOnPlayerCloseChannelEventHandler,
    NpcOnSellItemEventHandler npcOnSellItemEventHandler,
    NpcOnBuyItemEventHandler npcOnBuyItemEventHandler) : ICreatureEventSubscriber, IGameEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        creature.OnThink += creatureOnThinkEventHandler.Execute;

        if (creature is ICombatActor actor) actor.OnBeforeDeath += creatureOnPrepareDeathEventHandler.Execute;

        if (creature is IPlayer player)
        {
            player.OnWroteText += playerOnTextEditEventHandler.Execute;
        }

        if (creature is INpc npc)
        {
            npc.OnAppear += creatureOnAppearEventHandler.Execute;
            npc.OnDisappear += creatureOnDisappearEventHandler.Execute;
            npc.OnCreatureMove += creatureOnMoveEventHandler.Execute;

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
        creature.OnThink -= creatureOnThinkEventHandler.Execute;

        if (creature is ICombatActor actor) actor.OnBeforeDeath -= creatureOnPrepareDeathEventHandler.Execute;

        if (creature is IPlayer player)
        {
            player.OnWroteText -= playerOnTextEditEventHandler.Execute;
        }

        if (creature is INpc npc)
        {
            npc.OnAppear -= creatureOnAppearEventHandler.Execute;
            npc.OnDisappear -= creatureOnDisappearEventHandler.Execute;
            npc.OnCreatureMove -= creatureOnMoveEventHandler.Execute;

            npc.OnPlayerCloseChannel -= npcOnPlayerCloseChannelEventHandler.Execute;
        }

        if (creature is IShopperNpc npcShopper)
        {
            npcShopper.OnSellItem -= npcOnSellItemEventHandler.Execute;
            npcShopper.OnBuyItem -= npcOnBuyItemEventHandler.Execute;
        }
    }
}