using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Creature;
using NeoServer.Server.Events.Creature.Npcs;
using NeoServer.Server.Events.Talks;

namespace NeoServer.Server.Events.Subscribers;

public class CreatureEventSubscriber(
    CreatureBlockedAttackEventHandler creatureBlockedAttackEventHandler,
    CreatureTurnedToDirectionEventHandler creatureTurnToDirectionEventHandler,
    CreatureStartedWalkingEventHandler creatureStartedWalkingEventHandler,
    CreatureHealedEventHandler creatureHealedEventHandler,
    CreatureChangedAttackTargetEventHandler creatureChangedAttackTargetEventHandler,
    CreatureChangedSpeedEventHandler creatureChangedSpeedEventHandler,
    CreatureHearEventHandler creatureHearEventHandler,
    CreatureChangedOutfitEventHandler creatureChangedOutfitEventHandler,
    NpcShowShopEventHandler npcShowShopEventHandler,
    NpcCloseShopEventHandler npcCloseShopEventHandler)
    : ICreatureEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        creature.OnChangedOutfit += creatureChangedOutfitEventHandler.Execute;

        if (creature is ISociableCreature sociableCreature)
            sociableCreature.OnHear += creatureHearEventHandler.Execute;

        SubscribeToCombatActor(creature);

        if (creature is IShopperNpc shopperNpc)
        {
            shopperNpc.OnShowShop += npcShowShopEventHandler.Execute;
            shopperNpc.OnCloseShop += npcCloseShopEventHandler.Execute;
        }

        #region WalkableEvents

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnChangedSpeed += creatureChangedSpeedEventHandler.Execute;
            walkableCreature.OnStartedWalking += creatureStartedWalkingEventHandler.Execute;
            walkableCreature.OnTurnedToDirection += creatureTurnToDirectionEventHandler.Execute;
        }

        #endregion
    }

    public void Unsubscribe(ICreature creature)
    {
        creature.OnChangedOutfit -= creatureChangedOutfitEventHandler.Execute;

        if (creature is ICombatActor combatActor)
        {
            combatActor.OnTargetChanged -= creatureChangedAttackTargetEventHandler.Execute;
            combatActor.OnBlockedAttack -= creatureBlockedAttackEventHandler.Execute;
            combatActor.OnHeal -= creatureHealedEventHandler.Execute;
        }

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnChangedSpeed -= creatureChangedSpeedEventHandler.Execute;
            walkableCreature.OnTurnedToDirection -= creatureTurnToDirectionEventHandler.Execute;
            walkableCreature.OnStartedWalking -= creatureStartedWalkingEventHandler.Execute;
        }

        if (creature is ISociableCreature sociableCreature)
            sociableCreature.OnHear -= creatureHearEventHandler.Execute;
        if (creature is IShopperNpc shopperNpc) shopperNpc.OnShowShop -= npcShowShopEventHandler.Execute;
    }

    private void SubscribeToCombatActor(ICreature creature)
    {
        if (creature is not ICombatActor combatActor) return;

        combatActor.OnTargetChanged += creatureChangedAttackTargetEventHandler.Execute;
        combatActor.OnBlockedAttack += creatureBlockedAttackEventHandler.Execute;
        combatActor.OnHeal += creatureHealedEventHandler.Execute;
    }
}