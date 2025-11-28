using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Networking.EventHandlers.Creature;
using NeoServer.Networking.EventHandlers.Creature.Player;
using NeoServer.Server.Events.Chat;
using NeoServer.Server.Events.Combat;
using NeoServer.Server.Events.Items;
using NeoServer.Server.Events.Player;
using NeoServer.Server.Events.Player.Containers;
using NeoServer.Server.Events.Player.Party;

namespace NeoServer.Server.Events.Subscribers;

public class PlayerEventSubscriber(
    PlayerWalkCancelledEventHandler playerWalkCancelledEventHandler,
    PlayerClosedContainerEventHandler playerClosedContainerEventHandler,
    PlayerOpenedContainerEventHandler playerOpenedContainerEventHandler,
    ContentModifiedOnContainerEventHandler contentModifiedOnContainerEventHandler,
    PlayerChangedInventoryEventHandler itemAddedToInventoryEventHandler,
    InvalidOperationEventHandler invalidOperationEventHandler,
    CreatureStoppedAttackEventHandler creatureStoppedAttackEventHandler,
    PlayerGainedExperienceEventHandler playerGainedExperienceEventHandler,
    PlayerManaChangedEventHandler playerManaReducedEventHandler,
    SpellInvokedEventHandler playerUsedSpellEventHandler,
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
    PlayerSkullUpdatedEventHandler playerSkullUpdatedEventHandler)
    : ICreatureEventSubscriber
{
    public void Subscribe(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        player.OnStoppedWalking += playerWalkCancelledEventHandler.Execute;
        player.OnCancelledWalking += playerWalkCancelledEventHandler.Execute;
        player.Containers.OnClosedContainer += playerClosedContainerEventHandler.Execute;
        player.Containers.OnOpenedContainer += playerOpenedContainerEventHandler.Execute;

        player.Containers.RemoveItemAction += (owner, containerId, slotIndex, item) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemRemoved, containerId,
                slotIndex, item);

        player.Containers.AddItemAction += (owner, containerId, item) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemAdded, containerId, 0,
                item);

        player.Containers.UpdateItemAction += (owner, containerId, slotIndex, item, _) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemUpdated, containerId,
                slotIndex, item);

        player.Inventory.OnItemAddedToSlot +=
            itemAddedToInventoryEventHandler.Execute;
        player.Inventory.OnItemRemovedFromSlot +=
            itemAddedToInventoryEventHandler.Execute;

        player.Inventory.OnWeightChanged += itemAddedToInventoryEventHandler.ExecuteOnWeightChanged;

        player.Inventory.OnFailedToAddToSlot += invalidOperationEventHandler.Execute;
        player.OnStoppedAttack += creatureStoppedAttackEventHandler.Execute;
        player.OnAttackCanceled += creatureStoppedAttackEventHandler.Execute;
        player.OnGainedExperience += playerGainedExperienceEventHandler.Execute;

        player.OnStatusChanged += playerManaReducedEventHandler.Execute;
        player.OnUsedSpell += playerUsedSpellEventHandler.Execute;
        player.OnLevelAdvanced += playerLevelAdvancedEventHandler.Execute;
        player.OnLevelRegressed += playerLevelRegressedEventHandler.Execute;
        player.OnLookedAt += playerLookedAtEventHandler.Execute;
        player.OnGainedSkillPoint += playerUpdatedSkillPointsEventHandler.Execute;
        player.OnUsedItem += playerUsedItemEventHandler.Execute;
        player.PlayerSkull.OnSkullUpdated += playerSkullUpdatedEventHandler.Execute;

        player.Channels.OnJoinedChannel += playerJoinedChannelEventHandler.Execute;
        player.Channels.OnExitedChannel += playerExitedChannelEventHandler.Execute;
        player.Vip.OnAddedToVipList += playerAddedToVipListEventHandler.Execute;
        player.Vip.OnLoadedVipList += playerLoadedVipListEvent.Execute;
        player.OnChangedOnlineStatus += playerChangedOnlineStatusEventHandler.Execute;
        player.OnSentMessage += playerSentMessageEventHandler.Execute;
        player.PlayerParty.OnInviteToParty += playerInviteToPartyEventHandler.Execute;
        player.PlayerParty.OnRevokePartyInvite += playerRevokedPartyInviteEventHandler.Execute;
        player.PlayerParty.OnLeftParty += playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnInvitedToParty += playerInvitedToPartyEventHandler.Execute;
        player.PlayerParty.OnRejectedPartyInvite += playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnJoinedParty += playerJoinedPartyEventHandler.Execute;
        player.PlayerParty.OnPassedPartyLeadership += playerPassedPartyLeadershipEventHandler.Execute;
        player.OnExhausted += playerExhaustedEventHandler.Execute;
        player.OnAddedSkillBonus += playerUpdatedSkillPointsEventHandler.Execute;
        player.OnRemovedSkillBonus += playerUpdatedSkillPointsEventHandler.Execute;
    }

    public void Unsubscribe(ICreature creature)
    {
        if (creature is not IPlayer player) return;

        player.OnStoppedWalking -= playerWalkCancelledEventHandler.Execute;
        player.OnCancelledWalking -= playerWalkCancelledEventHandler.Execute;

        player.Containers.OnClosedContainer -= playerClosedContainerEventHandler.Execute;
        player.Containers.OnOpenedContainer -= playerOpenedContainerEventHandler.Execute;

        player.Containers.RemoveItemAction -= (owner, containerId, slotIndex, item) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemRemoved, containerId,
                slotIndex, item);

        player.Containers.AddItemAction -= (owner, containerId, item) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemAdded, containerId, 0,
                item);

        player.Containers.UpdateItemAction -= (owner, containerId, slotIndex, item, _) =>
            contentModifiedOnContainerEventHandler.Execute(owner, ContainerOperation.ItemUpdated, containerId,
                slotIndex, item);

        player.Inventory.OnItemAddedToSlot -=
            itemAddedToInventoryEventHandler.Execute;
        player.Inventory.OnItemRemovedFromSlot -=
            itemAddedToInventoryEventHandler.Execute;

        player.Inventory.OnFailedToAddToSlot -= invalidOperationEventHandler.Execute;
        player.OnStoppedAttack -= creatureStoppedAttackEventHandler.Execute;
        player.OnAttackCanceled -= creatureStoppedAttackEventHandler.Execute;
        player.OnGainedExperience -= playerGainedExperienceEventHandler.Execute;

        player.OnStatusChanged -= playerManaReducedEventHandler.Execute;
        player.OnUsedSpell -= playerUsedSpellEventHandler.Execute;
        player.OnLevelAdvanced -= playerLevelAdvancedEventHandler.Execute;
        player.OnLevelRegressed -= playerLevelRegressedEventHandler.Execute;
        player.OnLookedAt -= playerLookedAtEventHandler.Execute;
        player.OnGainedSkillPoint -= playerUpdatedSkillPointsEventHandler.Execute;
        player.OnUsedItem -= playerUsedItemEventHandler.Execute;
        player.PlayerSkull.OnSkullUpdated -= playerSkullUpdatedEventHandler.Execute;

        player.Channels.OnJoinedChannel -= playerJoinedChannelEventHandler.Execute;
        player.Channels.OnExitedChannel -= playerExitedChannelEventHandler.Execute;
        player.Vip.OnAddedToVipList -= playerAddedToVipListEventHandler.Execute;
        player.Vip.OnLoadedVipList -= playerLoadedVipListEvent.Execute;
        player.OnChangedOnlineStatus -= playerChangedOnlineStatusEventHandler.Execute;
        player.OnSentMessage -= playerSentMessageEventHandler.Execute;
        player.PlayerParty.OnInviteToParty -= playerInviteToPartyEventHandler.Execute;
        player.PlayerParty.OnRevokePartyInvite -= playerRevokedPartyInviteEventHandler.Execute;
        player.PlayerParty.OnLeftParty -= playerLeftPartyEventHandler.Execute;
        player.PlayerParty.OnInvitedToParty -= playerInvitedToPartyEventHandler.Execute;
        player.PlayerParty.OnJoinedParty -= playerJoinedPartyEventHandler.Execute;
        player.PlayerParty.OnPassedPartyLeadership -= playerPassedPartyLeadershipEventHandler.Execute;

        player.OnAddedSkillBonus -= playerUpdatedSkillPointsEventHandler.Execute;
        player.OnRemovedSkillBonus += playerUpdatedSkillPointsEventHandler.Execute;
        player.Inventory.OnWeightChanged -= itemAddedToInventoryEventHandler.ExecuteOnWeightChanged;
    }

    #region event handlers

    private readonly PlayerConditionChangedEventHandler _playerConditionChangedEventHandler;

    #endregion
}