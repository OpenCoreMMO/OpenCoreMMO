using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Creature;
using NeoServer.Server.Events.Creature.Npcs;

namespace NeoServer.Server.Events.Subscribers;

public class CreatureEventSubscriber(
    CreatureHealedEventHandler creatureHealedEventHandler,
    CreatureChangedAttackTargetEventHandler creatureChangedAttackTargetEventHandler,
    NpcShowShopEventHandler npcShowShopEventHandler,
    NpcCloseShopEventHandler npcCloseShopEventHandler)
    : ICreatureEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        SubscribeToCombatActor(creature);

        if (creature is IShopperNpc shopperNpc)
        {
            shopperNpc.OnShowShop += npcShowShopEventHandler.Execute;
            shopperNpc.OnCloseShop += npcCloseShopEventHandler.Execute;
        }
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is ICombatActor combatActor)
        {
            combatActor.OnTargetChanged -= creatureChangedAttackTargetEventHandler.Execute;
            combatActor.OnHeal -= creatureHealedEventHandler.Execute;
        }

        if (creature is IShopperNpc shopperNpc) shopperNpc.OnShowShop -= npcShowShopEventHandler.Execute;
    }

    private void SubscribeToCombatActor(ICreature creature)
    {
        if (creature is not ICombatActor combatActor) return;

        combatActor.OnTargetChanged += creatureChangedAttackTargetEventHandler.Execute;
        combatActor.OnHeal += creatureHealedEventHandler.Execute;
    }
}
