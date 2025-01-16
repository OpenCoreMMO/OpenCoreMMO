using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.EventHandlers.Creature;
using NeoServer.Networking.EventHandlers.Creature.Player;
using NeoServer.Server.Events.Chat;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Items;
using NeoServer.Server.Events.Player;
using NeoServer.Server.Events.Player.Containers;
using NeoServer.Server.Events.Player.Party;

namespace NeoServer.Server.Events.Subscribers;

public class PlayerEventSubscriber : ICreatureEventSubscriber
{
    public PlayerEventSubscriber(PlayerWalkCancelledEventHandler playerWalkCancelledEventHandler,
        PlayerClosedContainerEventHandler playerClosedContainerEventHandler,
        PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler,
        ContentModifiedOnContainerEventHandler contentModifiedOnContainerEventHandler,
        PlayerChangedInventoryEventHandler itemAddedToInventoryEventHandler,
        InvalidOperationEventHandler invalidOperationEventHandler,
        CreatureStoppedAttackEventHandler creatureStoppedAttackEventHandler,
        PlayerGainedExperienceEventHandler playerGainedExperienceEventHandler,
        PlayerManaChangedEventHandler playerManaReducedEventHandler,
        SpellInvokedEventHandler playerUsedSpellEventHandler,
        PlayerCannotUseSpellEventHandler playerCannotUseSpellEventHandler,
        PlayerConditionChangedEventHandler playerConditionChangedEventHandler,
        PlayerLevelAdvancedEventHandler playerLevelAdvancedEventHandler,
        PlayerLevelRegressedEventHandler playerLevelRegressedEventHandler,
        PlayerLookedAtEventHandler playerLookedAtEventHandler,
        PlayerUpdatedSkillPointsEventHandler playerUpdatedSkillPointsEventHandler,
        PlayerUsedItemEventHandler playerUsedItemEventHandler,
        PlayerJoinedChannelEventHandler playerJoinedChannelEventHandler,
        PlayerExitedChannelEventHandler playerExitedChannelEventHandler,
        PlayerAddToVipListEventHandler playerAddedToVipListEventHandler,
        PlayerLoadedVipListEventHandler playerLoadedVipListEvent,
        PlayerChangedOnlineStatusEventHandler playerChangedOnlineStatusEventHandler,
        PlayerSentMessageEventHandler playerSentMessageEventHandler,
        PlayerInviteToPartyEventHandler playerInviteToPartyEventHandler,
        PlayerRevokedPartyInviteEventHandler playerRevokedPartyInviteEventHandler,
        PlayerLeftPartyEventHandler playerLeftPartyEventHandler,
        PlayerInvitedToPartyEventHandler playerInvitedToPartyEventHandler,
        PlayerJoinedPartyEventHandler playerJoinedPartyEventHandler,
        PlayerPassedPartyLeadershipEventHandler playerPassedPartyLeadershipEventHandler,
        PlayerExhaustedEventHandler playerExhaustedEventHandler,
        PlayerReadTextEventHandler playerReadTextEventHandler,
        PlayerLoggedInEventHandler playerLoggedInEventHandler,
        PlayerLoggedOutEventHandler playerLoggedOutEventHandler, PlayerSkullUpdatedEventHandler playerSkullUpdatedEventHandler)
    {
        _playerWalkCancelledEventHandler = playerWalkCancelledEventHandler;
        _playerClosedContainerEventHandler = playerClosedContainerEventHandler;
        _playerOpenedContainerEventHandler = playerOpenedContainerEventHandler;
        _contentModifiedOnContainerEventHandler = contentModifiedOnContainerEventHandler;
        _itemAddedToInventoryEventHandler = itemAddedToInventoryEventHandler;
        _invalidOperationEventHandler = invalidOperationEventHandler;
        _creatureStoppedAttackEventHandler = creatureStoppedAttackEventHandler;
        _playerGainedExperienceEventHandler = playerGainedExperienceEventHandler;
        _playerManaReducedEventHandler = playerManaReducedEventHandler;
        _playerUsedSpellEventHandler = playerUsedSpellEventHandler;
        _playerCannotUseSpellEventHandler = playerCannotUseSpellEventHandler;
        _playerConditionChangedEventHandler = playerConditionChangedEventHandler;
        _playerLevelAdvancedEventHandler = playerLevelAdvancedEventHandler;
        _playerLevelRegressedEventHandler = playerLevelRegressedEventHandler;
        _playerLookedAtEventHandler = playerLookedAtEventHandler;
        _playerUpdatedSkillPointsEventHandler = playerUpdatedSkillPointsEventHandler;
        _playerUsedItemEventHandler = playerUsedItemEventHandler;
        _playerJoinedChannelEventHandler = playerJoinedChannelEventHandler;
        _playerExitedChannelEventHandler = playerExitedChannelEventHandler;
        _playerAddedToVipListEventHandler = playerAddedToVipListEventHandler;
        _playerLoadedVipListEvent = playerLoadedVipListEvent;
        _playerChangedOnlineStatusEventHandler = playerChangedOnlineStatusEventHandler;
        _playerSentMessageEventHandler = playerSentMessageEventHandler;
        _playerInviteToPartyEventHandler = playerInviteToPartyEventHandler;
        _playerRevokedPartyInviteEventHandler = playerRevokedPartyInviteEventHandler;
        _playerLeftPartyEventHandler = playerLeftPartyEventHandler;
        _playerInvitedToPartyEventHandler = playerInvitedToPartyEventHandler;
        _playerJoinedPartyEventHandler = playerJoinedPartyEventHandler;
        _playerPassedPartyLeadershipEventHandler = playerPassedPartyLeadershipEventHandler;
        _playerExhaustedEventHandler = playerExhaustedEventHandler;
        _playerReadTextEventHandler = playerReadTextEventHandler;
        _playerLoggedInEventHandler = playerLoggedInEventHandler;
        _playerLoggedOutEventHandler = playerLoggedOutEventHandler;
        _playerSkullUpdatedEventHandler = playerSkullUpdatedEventHandler;
    }

    public void Subscribe(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        player.OnStoppedWalking += _playerWalkCancelledEventHandler.Execute;
        player.OnCancelledWalking += _playerWalkCancelledEventHandler.Execute;
        player.Containers.OnClosedContainer += _playerClosedContainerEventHandler.Execute;
        player.Containers.OnOpenedContainer += _playerOpenedContainerEventHandler.Execute;

        player.Containers.RemoveItemAction += (owner, containerId, slotIndex, item) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemRemoved, containerId,
                slotIndex, item);

        player.Containers.AddItemAction += (owner, containerId, item) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemAdded, containerId, 0,
                item);

        player.Containers.UpdateItemAction += (owner, containerId, slotIndex, item, _) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemUpdated, containerId,
                slotIndex, item);

        player.Inventory.OnItemAddedToSlot +=
            _itemAddedToInventoryEventHandler.Execute;
        player.Inventory.OnItemRemovedFromSlot +=
            _itemAddedToInventoryEventHandler.Execute;

        player.Inventory.OnWeightChanged += _itemAddedToInventoryEventHandler.ExecuteOnWeightChanged;

        player.Inventory.OnFailedToAddToSlot += _invalidOperationEventHandler.Execute;
        player.OnStoppedAttack += _creatureStoppedAttackEventHandler.Execute;
        player.OnAttackCanceled += _creatureStoppedAttackEventHandler.Execute;
        player.OnGainedExperience += _playerGainedExperienceEventHandler.Execute;

        player.OnStatusChanged += _playerManaReducedEventHandler.Execute;
        player.OnUsedSpell += _playerUsedSpellEventHandler.Execute;
        player.OnCannotUseSpell += _playerCannotUseSpellEventHandler.Execute;
        player.OnLevelAdvanced += _playerLevelAdvancedEventHandler.Execute;
        player.OnLevelRegressed += _playerLevelRegressedEventHandler.Execute;
        player.OnLookedAt += _playerLookedAtEventHandler.Execute;
        player.OnGainedSkillPoint += _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnUsedItem += _playerUsedItemEventHandler.Execute;
        player.PlayerSkull.OnSkullUpdated += _playerSkullUpdatedEventHandler.Execute;

        player.OnLoggedIn += _playerLoggedInEventHandler.Execute;
        player.OnLoggedOut += _playerLoggedOutEventHandler.Execute;

        player.Channels.OnJoinedChannel += _playerJoinedChannelEventHandler.Execute;
        player.Channels.OnExitedChannel += _playerExitedChannelEventHandler.Execute;
        player.Vip.OnAddedToVipList += _playerAddedToVipListEventHandler.Execute;
        player.Vip.OnLoadedVipList += _playerLoadedVipListEvent.Execute;
        player.OnChangedOnlineStatus += _playerChangedOnlineStatusEventHandler.Execute;
        player.OnSentMessage += _playerSentMessageEventHandler.Execute;
        player.PlayerParty.OnInviteToParty += _playerInviteToPartyEventHandler.Execute;
        player.PlayerParty.OnRevokePartyInvite += _playerRevokedPartyInviteEventHandler.Execute;
        player.PlayerParty.OnLeftParty += _playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnInvitedToParty += _playerInvitedToPartyEventHandler.Execute;
        player.PlayerParty.OnRejectedPartyInvite += _playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnJoinedParty += _playerJoinedPartyEventHandler.Execute;
        player.PlayerParty.OnPassedPartyLeadership += _playerPassedPartyLeadershipEventHandler.Execute;
        player.OnExhausted += _playerExhaustedEventHandler.Execute;
        player.OnAddedSkillBonus += _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnRemovedSkillBonus += _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnReadText += _playerReadTextEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        player.OnStoppedWalking -= _playerWalkCancelledEventHandler.Execute;
        player.OnCancelledWalking -= _playerWalkCancelledEventHandler.Execute;

        player.Containers.OnClosedContainer -= _playerClosedContainerEventHandler.Execute;
        player.Containers.OnOpenedContainer -= _playerOpenedContainerEventHandler.Execute;

        player.Containers.RemoveItemAction -= (owner, containerId, slotIndex, item) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemRemoved, containerId,
                slotIndex, item);

        player.Containers.AddItemAction -= (owner, containerId, item) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemAdded, containerId, 0,
                item);

        player.Containers.UpdateItemAction -= (owner, containerId, slotIndex, item, _) =>
            _contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemUpdated, containerId,
                slotIndex, item);

        player.Inventory.OnItemAddedToSlot -=
            _itemAddedToInventoryEventHandler.Execute;
        player.Inventory.OnItemRemovedFromSlot -=
            _itemAddedToInventoryEventHandler.Execute;

        player.Inventory.OnFailedToAddToSlot -= _invalidOperationEventHandler.Execute;
        player.OnStoppedAttack -= _creatureStoppedAttackEventHandler.Execute;
        player.OnAttackCanceled -= _creatureStoppedAttackEventHandler.Execute;
        player.OnGainedExperience -= _playerGainedExperienceEventHandler.Execute;

        player.OnStatusChanged -= _playerManaReducedEventHandler.Execute;
        player.OnUsedSpell -= _playerUsedSpellEventHandler.Execute;
        player.OnCannotUseSpell -= _playerCannotUseSpellEventHandler.Execute;
        player.OnLevelAdvanced -= _playerLevelAdvancedEventHandler.Execute;
        player.OnLevelRegressed -= _playerLevelRegressedEventHandler.Execute;
        player.OnLookedAt -= _playerLookedAtEventHandler.Execute;
        player.OnGainedSkillPoint -= _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnUsedItem -= _playerUsedItemEventHandler.Execute;
        player.PlayerSkull.OnSkullUpdated -= _playerSkullUpdatedEventHandler.Execute;

        player.OnLoggedIn -= _playerLoggedInEventHandler.Execute;
        player.OnLoggedOut -= _playerLoggedOutEventHandler.Execute;

        player.Channels.OnJoinedChannel -= _playerJoinedChannelEventHandler.Execute;
        player.Channels.OnExitedChannel -= _playerExitedChannelEventHandler.Execute;
        player.Vip.OnAddedToVipList -= _playerAddedToVipListEventHandler.Execute;
        player.Vip.OnLoadedVipList -= _playerLoadedVipListEvent.Execute;
        player.OnChangedOnlineStatus -= _playerChangedOnlineStatusEventHandler.Execute;
        player.OnSentMessage -= _playerSentMessageEventHandler.Execute;
        player.PlayerParty.OnInviteToParty -= _playerInviteToPartyEventHandler.Execute;
        player.PlayerParty.OnRevokePartyInvite -= _playerRevokedPartyInviteEventHandler.Execute;
        player.PlayerParty.OnLeftParty -= _playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnInvitedToParty -= _playerInvitedToPartyEventHandler.Execute;
        player.PlayerParty.OnJoinedParty -= _playerJoinedPartyEventHandler.Execute;
        player.PlayerParty.OnPassedPartyLeadership -= _playerPassedPartyLeadershipEventHandler.Execute;

        player.OnAddedSkillBonus -= _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnRemovedSkillBonus += _playerUpdatedSkillPointsEventHandler.Execute;
        player.OnReadText -= _playerReadTextEventHandler.Execute;
        player.Inventory.OnWeightChanged -= _itemAddedToInventoryEventHandler.ExecuteOnWeightChanged;
    }

    #region event handlers

    private readonly PlayerWalkCancelledEventHandler _playerWalkCancelledEventHandler;
    private readonly PlayerClosedContainerEventHandler _playerClosedContainerEventHandler;
    private readonly PlayerOpenedContainerEventHandler _playerOpenedContainerEventHandler;
    private readonly ContentModifiedOnContainerEventHandler _contentModifiedOnContainerEventHandler;
    private readonly PlayerChangedInventoryEventHandler _itemAddedToInventoryEventHandler;
    private readonly InvalidOperationEventHandler _invalidOperationEventHandler;
    private readonly CreatureStoppedAttackEventHandler _creatureStoppedAttackEventHandler;
    private readonly PlayerGainedExperienceEventHandler _playerGainedExperienceEventHandler;
    private readonly PlayerManaChangedEventHandler _playerManaReducedEventHandler;
    private readonly SpellInvokedEventHandler _playerUsedSpellEventHandler;
    private readonly PlayerCannotUseSpellEventHandler _playerCannotUseSpellEventHandler;
    private readonly PlayerConditionChangedEventHandler _playerConditionChangedEventHandler;
    private readonly PlayerLevelAdvancedEventHandler _playerLevelAdvancedEventHandler;
    private readonly PlayerLevelRegressedEventHandler _playerLevelRegressedEventHandler;
    private readonly PlayerLookedAtEventHandler _playerLookedAtEventHandler;
    private readonly PlayerUpdatedSkillPointsEventHandler _playerUpdatedSkillPointsEventHandler;
    private readonly PlayerUsedItemEventHandler _playerUsedItemEventHandler;
    private readonly PlayerJoinedChannelEventHandler _playerJoinedChannelEventHandler;
    private readonly PlayerExitedChannelEventHandler _playerExitedChannelEventHandler;
    private readonly PlayerAddToVipListEventHandler _playerAddedToVipListEventHandler;
    private readonly PlayerLoadedVipListEventHandler _playerLoadedVipListEvent;
    private readonly PlayerChangedOnlineStatusEventHandler _playerChangedOnlineStatusEventHandler;
    private readonly PlayerSentMessageEventHandler _playerSentMessageEventHandler;
    private readonly PlayerInviteToPartyEventHandler _playerInviteToPartyEventHandler;
    private readonly PlayerRevokedPartyInviteEventHandler _playerRevokedPartyInviteEventHandler;
    private readonly PlayerLeftPartyEventHandler _playerLeftPartyEventHandler;
    private readonly PlayerInvitedToPartyEventHandler _playerInvitedToPartyEventHandler;
    private readonly PlayerJoinedPartyEventHandler _playerJoinedPartyEventHandler;
    private readonly PlayerPassedPartyLeadershipEventHandler _playerPassedPartyLeadershipEventHandler;
    private readonly PlayerExhaustedEventHandler _playerExhaustedEventHandler;
    private readonly PlayerReadTextEventHandler _playerReadTextEventHandler;
    private readonly PlayerLoggedInEventHandler _playerLoggedInEventHandler;
    private readonly PlayerLoggedOutEventHandler _playerLoggedOutEventHandler;
    private readonly PlayerSkullUpdatedEventHandler _playerSkullUpdatedEventHandler;

    #endregion
}