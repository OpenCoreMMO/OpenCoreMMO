using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Creature;
using NeoServer.Server.Events.Creature.Npcs;
using NeoServer.Server.Events.Talks;
using CreatureInjuredEventHandler = NeoServer.Server.Events.Creature.CreatureInjuredEventHandler;

namespace NeoServer.Server.Events.Subscribers;

public class CreatureEventSubscriber : ICreatureEventSubscriber
{
    private readonly CreatureAttackEventHandler _creatureAttackEventHandler;
    private readonly CreatureBlockedAttackEventHandler _creatureBlockedAttackEventHandler;
    private readonly CreatureChangedAttackTargetEventHandler _creatureChangedAttackTargetEventHandler;
    private readonly CreatureChangedOutfitEventHandler _creatureChangedOutfitEventHandler;
    private readonly CreatureChangedSpeedEventHandler _creatureChangedSpeedEventHandler;
    private readonly CreatureHealedEventHandler _creatureHealedEventHandler;
    private readonly CreatureHearEventHandler _creatureHearEventHandler;
    private readonly CreatureStartedFollowingEventHandler _creatureStartedFollowingEventHandler;
    private readonly CreatureStartedWalkingEventHandler _creatureStartedWalkingEventHandler;
    private readonly CreatureChangedVisibilityEventHandler _creatureTurnedInvisibleEventHandler;
    private readonly CreatureTurnedToDirectionEventHandler _creatureTurnToDirectionEventHandler;
    private readonly NpcCloseShopEventHandler _npcCloseShopEventHandler;
    private readonly NpcShowShopEventHandler _npcShowShopEventHandler;

    public CreatureEventSubscriber(CreatureInjuredEventHandler creatureReceiveDamageEventHandler,
        CreatureBlockedAttackEventHandler creatureBlockedAttackEventHandler,
        CreatureAttackEventHandler creatureAttackEventHandler,
        CreatureTurnedToDirectionEventHandler creatureTurnToDirectionEventHandler,
        CreatureStartedWalkingEventHandler creatureStartedWalkingEventHandler,
        CreatureHealedEventHandler creatureHealedEventHandler,
        CreatureChangedAttackTargetEventHandler creatureChangedAttackTargetEventHandler,
        CreatureStartedFollowingEventHandler creatureStartedFollowingEventHandler,
        CreatureChangedSpeedEventHandler creatureChangedSpeedEventHandler,
        CreatureHearEventHandler creatureHearEventHandler,
        CreatureChangedVisibilityEventHandler creatureTurnedInvisibleEventHandler,
        CreatureChangedOutfitEventHandler creatureChangedOutfitEventHandler,
        NpcShowShopEventHandler npcShowShopEventHandler,
        NpcCloseShopEventHandler npcCloseShopEventHandler)
    {
        _creatureBlockedAttackEventHandler = creatureBlockedAttackEventHandler;
        _creatureAttackEventHandler = creatureAttackEventHandler;
        _creatureTurnToDirectionEventHandler = creatureTurnToDirectionEventHandler;
        _creatureStartedWalkingEventHandler = creatureStartedWalkingEventHandler;
        _creatureHealedEventHandler = creatureHealedEventHandler;
        _creatureChangedAttackTargetEventHandler = creatureChangedAttackTargetEventHandler;
        _creatureStartedFollowingEventHandler = creatureStartedFollowingEventHandler;
        _creatureChangedSpeedEventHandler = creatureChangedSpeedEventHandler;
        _creatureHearEventHandler = creatureHearEventHandler;
        _creatureTurnedInvisibleEventHandler = creatureTurnedInvisibleEventHandler;
        _creatureChangedOutfitEventHandler = creatureChangedOutfitEventHandler;
        _npcShowShopEventHandler = npcShowShopEventHandler;
        _npcCloseShopEventHandler = npcCloseShopEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        creature.OnChangedOutfit += _creatureChangedOutfitEventHandler.Execute;

        if (creature is ISociableCreature sociableCreature)
            sociableCreature.OnHear += _creatureHearEventHandler.Execute;

        SubscribeToCombatActor(creature);

        if (creature is IShopperNpc shopperNpc)
        {
            shopperNpc.OnShowShop += _npcShowShopEventHandler.Execute;
            shopperNpc.OnCloseShop += _npcCloseShopEventHandler.Execute;
        }

        #region WalkableEvents

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnStartedFollowing += _creatureStartedFollowingEventHandler.Execute;
            walkableCreature.OnChangedSpeed += _creatureChangedSpeedEventHandler.Execute;
            walkableCreature.OnStartedWalking += _creatureStartedWalkingEventHandler.Execute;
            walkableCreature.OnTurnedToDirection += _creatureTurnToDirectionEventHandler.Execute;
        }

        #endregion
    }

    public void Unsubscribe(ICreature creature)
    {
        creature.OnChangedOutfit -= _creatureChangedOutfitEventHandler.Execute;

        if (creature is ICombatActor combatActor)
        {
            combatActor.OnTargetChanged -= _creatureChangedAttackTargetEventHandler.Execute;
            combatActor.OnBlockedAttack -= _creatureBlockedAttackEventHandler.Execute;
            combatActor.OnAttackEnemy -= _creatureAttackEventHandler.Execute;
            combatActor.OnHeal -= _creatureHealedEventHandler.Execute;
            combatActor.OnChangedVisibility -= _creatureTurnedInvisibleEventHandler.Execute;
        }

        if (creature is IWalkableCreature walkableCreature)
        {
            walkableCreature.OnStartedFollowing -= _creatureStartedFollowingEventHandler.Execute;
            walkableCreature.OnChangedSpeed -= _creatureChangedSpeedEventHandler.Execute;
            walkableCreature.OnTurnedToDirection -= _creatureTurnToDirectionEventHandler.Execute;
            walkableCreature.OnStartedWalking -= _creatureStartedWalkingEventHandler.Execute;
        }

        if (creature is ISociableCreature sociableCreature)
            sociableCreature.OnHear -= _creatureHearEventHandler.Execute;
        if (creature is IShopperNpc shopperNpc) shopperNpc.OnShowShop -= _npcShowShopEventHandler.Execute;
    }

    private void SubscribeToCombatActor(ICreature creature)
    {
        if (creature is not ICombatActor combatActor) return;

        combatActor.OnTargetChanged += _creatureChangedAttackTargetEventHandler.Execute;
        combatActor.OnBlockedAttack += _creatureBlockedAttackEventHandler.Execute;
        combatActor.OnAttackEnemy += _creatureAttackEventHandler.Execute;
        combatActor.OnHeal += _creatureHealedEventHandler.Execute;
        combatActor.OnChangedVisibility += _creatureTurnedInvisibleEventHandler.Execute;
    }
}