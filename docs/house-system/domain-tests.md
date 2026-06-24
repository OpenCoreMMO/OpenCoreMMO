# House System — Domain Unit Tests (Expected Output)

> Phase 1 reference — 1:1 with implemented tests after review fixes (spec `review-fixes-spec.md`).
> Every test name here corresponds to a real test in `tests/NeoServer.Domain.Tests/Houses/`.

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

> Note (m2): `PayRent(owner, now, rentPeriodSeconds)` — `ICoinTypeStore` was intentionally
> removed in Phase 1. Rent is bank-balance based (`owner.Bank.Debit`). If a future phase needs
> coin-specific rent, re-introduce the store at the service layer, not the aggregate.

- PayRent_NotDue_ReturnsNotDue_NoDeduction → NotDue, bank unchanged.
- PayRent_DueAndSufficientBank_DeductsRentAndAdvancesPaidUntil → Paid, bank -= rent, PaidUntil advanced.
- PayRent_DueAndSufficientBank_ResetsWarningsToZero → Paid, warnings == 0.
- PayRent_DueAndInsufficientBank_IncrementsWarnings → Warned, warnings +1.
- PayRent_SeventhWarning_ReturnsEvicted → Evicted, OwnerGuid == 0.
- PayRent_RentZero_ReturnsNotDue → NotDue.
- PayRent_Unowned_ReturnsNotDue → NotDue.
- SetPayRentWarnings_AboveCap_ClampsToSeven → value == 7.

## HouseTileAssociationTests

> M1: `LinkTile` now sets `TileFlags.ProtectionZone` via `IDynamicTile.SetAsProtectionZone()`.
> M2: cross-house detection via `CanEnterFunction != null` guard (see code comment for Phase-2 upgrade path).

- LinkTile_InstallsCanEnterFunction_BlockingUninvitedPlayer → CanEnterFunction false for stranger.
- LinkTile_CanEnterFunction_AllowsInvitedPlayer → CanEnterFunction true for owner.
- LinkTile_NonPlayerCreature_CanEnterFalse → CanEnterFunction false for non-player.
- GetTileCount_AfterLinkingTiles_ReturnsCount → TileCount matches linked count.
- LinkTile_SetsProtectionZoneFlag → after LinkTile, HasFlag(ProtectionZone) is true. (M1)
- LinkTile_SameTileToSameHouseTwice_Throws → InvalidOperationException when same tile linked to same house twice. (M2)
- LinkTile_SameTileToTwoDifferentHouses_Throws → InvalidOperationException when tile claimed by house A is linked to house B. (M2)

## HouseDoorBedTests
- GetDoorCount_AfterLinking_ReturnsCount → DoorCount matches linked doors.
- LinkBed_AddsBedToHouse_GetBedCountReflects → BedCount matches linked beds.
- SetAccessList_DoorListId_UpdatesThatDoorOnly → only that door's list changes, other doors unaffected.
- EntryPosition_DefaultsToFirstTileLocation → EntryPosition == first linked tile's Location.

## HouseFactoryTests
- Create_FromEntity_MapsIdNameTownRentWarningsOwner → all fields mapped from entity values.
- Create_UnownedEntity_ProducesUnownedHouseWithEmptyLists → OwnerGuid 0, OwnerName empty, no tiles/doors/beds.

## HouseItemMovementPolicyTests

> m1: `HouseItemMovementPolicy(IHouseStore)` implements the invited-only movement rule.
> DI registration deferred to Phase 2/3 movement-layer wiring.

- CanMoveItem_NonHouseTile_ReturnsTrue → GetByTile returns null → true for any player.
- CanMoveItem_HouseTile_InvitedPlayer_ReturnsTrue → store returns house where IsInvited is true → true.
- CanMoveItem_HouseTile_UninvitedPlayer_ReturnsFalse → store returns house where IsInvited is false → false.
- CanMoveItem_HouseTile_NullPlayer_ReturnsFalse → house tile + null player → false.
- CanMoveItem_NullTile_ReturnsTrue → no tile context → true.

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

## HousePremiumValidationTests

> Tests for `HouseService.CanPlayerOwnHouse(IPlayer)`. Validates that the centralized
> premium check respects the `HouseConfiguration.RequirePremiumAccount` flag in both states.
> Default config (`new HouseConfiguration()`) defaults to `RequirePremiumAccount: true`.

- CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsTrueForNonPremiumPlayer → non-premium player allowed when flag is false.
- CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsTrueForPremiumPlayer → premium player allowed when flag is false.
- CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsFalseForNonPremiumPlayer → non-premium player rejected when flag is true.
- CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsTrueForPremiumPlayer → premium player allowed when flag is true.
- CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsFalseForNullPlayer → null player rejected when flag is true.
- CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsFalseForNullPlayer → null player rejected when flag is false.
- CanPlayerOwnHouse_DefaultConfig_RequiresPremium → default HouseConfiguration enforces premium (non-premium rejected, premium allowed).

---

## LocationEqualityTests (Common/Structs)

> C1: fixed `Location.Equals(object)` infinite recursion (StackOverflow) and cleaned dead
> `NullReferenceException` catches from `operator ==`/`!=`.

- Equals_BoxedEqualLocation_ReturnsTrue → boxed compare of equal locations returns true (does not overflow).
- Equals_BoxedDifferentLocation_ReturnsFalse → boxed compare of different locations returns false.
- Equals_BoxedNonLocation_ReturnsFalse → comparing a Location to a non-Location object returns false.
- Equals_TypedOverload_MatchesOperator → `loc.Equals(other) == (loc == other)` for equal and unequal pairs.
- Be_ViaFluentAssertions_DoesNotOverflow → FluentAssertions `.Be()` passes for equal locations (regression guard).
- GetHashCode_EqualLocations_AreEqual → equal locations share the same hash code.
- OperatorEquality_And_Inequality_AreConsistent → `(a == b) == !(a != b)` for equal and unequal pairs.

---

## Deferred to Phase 2/3

The following cases are planned but not implemented in Phase 1. They are listed here to document
their target phase explicitly — none were silently dropped.

### GetDoorIdByPosition (Phase 3 dependency — m4)
- `GetDoorIdByPosition_KnownDoor_ReturnsDoorId` — deferred to Phase 3.
  Requires a reverse `Dictionary<Location, uint>` map populated at `LinkDoor` time.
  A door item's `Location` is only reliably available after the Phase-2 world-attach step
  (doors receive their location when linked from the map). Implementing in Phase 1 would require
  storing a `Location` per door at link time without a reliable source for it.
  See `House-System-Plan.md` Phase 3, `HouseFunctions.getDoorIdByPosition`.
- `GetDoorIdByPosition_NoDoorAtPosition_ReturnsZero` — same blocker; deferred to Phase 3.

### Factory access-list and door seeding (Phase 2 — HouseAccessListLoader)
- `Create_FromEntity_SeedsGuestList_*`, `Create_FromEntity_SeedsSubownerList_*`,
  `Create_FromEntity_SeedsDoorList_*` — deferred to Phase 2.
  The factory creates empty lists in Phase 1. `HouseAccessListLoader` (Phase 2, Loaders project)
  will populate access lists from `HouseListEntity` rows after the factory call.

### SetNewOwner_WakesAllLinkedBeds (covered by HouseServiceTests)
- This side effect lives in `HouseService`, not the aggregate. It is already tested by
  `HouseServiceTests.SetOwner_WakesAllBeds`. No separate aggregate test needed.
