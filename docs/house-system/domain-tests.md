# House System — Domain Unit Tests (Expected Output)

## HouseOwnershipTests
- SetNewOwner_FromUnownedToPlayer_SetsOwnerGuidNameAccount → OwnerGuid=10, OwnerName="NewOwner", OwnerAccountId=100.
- SetNewOwner_WithUpdatePaidUntilTrue_SetsPaidUntilToNowPlusRentPeriod → PaidUntil == now + 86400s.
- SetNewOwner_WithUpdatePaidUntilTrue_ResetsPayRentWarningsToZero → PayRentWarnings == 0.
- SetNewOwner_ChangingOwner_ClearsGuestAndSubownerLists → both lists null after change.
- SetNewOwner_ChangingOwner_ClearsAllDoorAccessLists → all door lists null after change.
- SetNewOwner_ToZeroGuid_MarksHouseUnowned → OwnerGuid=0, OwnerName empty, OwnerAccountId=0, lists cleared.
- SetNewOwner_ToSameGuid_DoesNotClearLists → lists intact, owner fields unchanged.
- SetNewOwner_SameGuidWithUpdatePaidUntil_RefreshesPaidUntil → PaidUntil advanced, warnings reset.

## HouseAccessListTests
- AddPlayer_AddsNameCaseInsensitive → IsInList true regardless of case.
- AllowAll_MakesAnyPlayerInList → IsInList true for any player.
- AddGuild_AddsGuildId → guild member IsInList true.
- AddGuildRank_MatchesRankLevelOrAbove → player with rank >= list rank IsInList true.
- AddGuildRank_LowerRankDoesNotMatch → player with rank < list rank IsInList false.
- Clear_ResetsAllEntries → IsInList false for all previously added entries.
- IsInList_PlayerNotInList_ReturnsFalse → false.
- AddPlayer_MultipleNames_AllMatch → IsInList true for all added names.

## HouseAccessLevelTests
- GetAccessLevel_Owner_ReturnsOwner → Owner.
- GetAccessLevel_PlayerInSubownerList_ReturnsSubOwner → SubOwner.
- GetAccessLevel_PlayerInGuestListOnly_ReturnsGuest → Guest.
- GetAccessLevel_PlayerInBothLists_ReturnsSubOwner → SubOwner (higher wins).
- GetAccessLevel_UninvitedPlayer_ReturnsNotInvited → NotInvited.
- IsInvited_GuestPlayer_ReturnsTrue → true.
- IsInvited_UninvitedPlayer_ReturnsFalse → false.
- CanEnter_InvitedPlayer_ReturnsTrue → true.
- CanEnter_UninvitedPlayer_ReturnsFalse → false.
- CanEnter_NonPlayerCreature_ReturnsFalse → false.
- CanEditAccessList_OwnerEditsSubownerList_ReturnsTrue → true.
- CanEditAccessList_SubownerEditsSubownerList_ReturnsFalse → false.
- CanEditAccessList_SubownerEditsGuestList_ReturnsTrue → true.
- CanEditAccessList_GuestEditsAnyList_ReturnsFalse → false for guest/subowner/door lists.

## HouseEvictionTests
- CanKick_OwnerKicksGuest_ReturnsTrue → true (caster >= SubOwner, level > target, target on house tile).
- CanKick_SubownerKicksGuest_ReturnsTrue → true.
- CanKick_GuestKicksAnyone_ReturnsFalse → false (caster access too low).
- CanKick_AnyoneKicksOwner_ReturnsFalse → false (cannot kick owner).
- CanKick_TargetNotInHouse_ReturnsFalse → false (target.Tile not in house tiles).

## HouseRentTests
- PayRent_NotDue_ReturnsNotDue_NoDeduction → NotDue, bank unchanged.
- PayRent_DueAndSufficientBank_DeductsRentAndAdvancesPaidUntil → Paid, bank -= rent, PaidUntil advanced.
- PayRent_DueAndSufficientBank_ResetsWarningsToZero → Paid, warnings == 0.
- PayRent_DueAndInsufficientBank_IncrementsWarnings → Warned, warnings +1.
- PayRent_SeventhWarning_ReturnsEvicted → Evicted, OwnerGuid == 0.
- PayRent_RentZero_ReturnsNotDue → NotDue.
- PayRent_Unowned_ReturnsNotDue → NotDue.
- SetPayRentWarnings_AboveCap_ClampsToSeven → value == 7.

## HouseTileAssociationTests
- LinkTile_InstallsCanEnterFunction_BlockingUninvitedPlayer → CanEnterFunction false for stranger.
- LinkTile_CanEnterFunction_AllowsInvitedPlayer → CanEnterFunction true for owner.
- LinkTile_NonPlayerCreature_CanEnterFalse → CanEnterFunction false for non-player.
- GetTileCount_AfterLinkingTiles_ReturnsCount → TileCount matches linked count.
- LinkTile_SameTileToTwoHouses_Throws → InvalidOperationException.

## HouseDoorBedTests
- GetDoorCount_AfterLinking_ReturnsCount → DoorCount matches linked doors.
- LinkBed_AddsBedToHouse_GetBedCountReflects → BedCount matches linked beds.
- SetAccessList_DoorListId_UpdatesThatDoorOnly → only that door's list changes, other doors unaffected.
- EntryPosition_DefaultsToFirstTileLocation → EntryPosition == first linked tile's Location.

## HouseFactoryTests
- Create_FromEntity_MapsIdNameTownRentWarningsOwner → all fields mapped from entity values.
- Create_UnownedEntity_ProducesUnownedHouseWithEmptyLists → OwnerGuid 0, OwnerName empty, no tiles/doors/beds.

## HouseServiceTests
- SetOwner_EvictsNowUninvitedPlayers → eviction seam called for each player on tiles who isn't new owner.
- SetOwner_WakesAllBeds → bed-waker seam called with all house beds.
- SetOwner_TransfersPickupableItemsToOldOwnerDepot → depot seam receives pickupable items only.
- SetOwner_LeavesNonPickupableItems → depot seam not called when only fixed items exist.
- SetOwner_PersistsHouse → repository.Save called after aggregate state change.
- SetOwner_RaisesHouseOwnerChangedEvent → event raised with old/new owner guids.
- PayRent_Warned_RaisesHouseRentWarningEvent → warning event raised with house, owner, warning number.
- PayRent_Evicted_RaisesHouseEvictedEvent → eviction event raised with house.
- PayRent_Paid_PersistsHouseState → repository.Save called after successful payment.
- CanKick_Success_TeleportsTargetAndPersists → eviction seam called + repository.Save.
- CanKick_Failure_NoSideEffects → no eviction or persist calls.
