# House System — Phase 1 Step-by-Step (Domain + Unit Tests)

> Companion to `humble-growing-pony.md`. This document breaks **Phase 1 only** into an ordered,
> verifiable checklist. Phase 1 is **pure domain**: no EF, no Lua, no map loading. Everything that
> needs the outside world (depot transfer, teleport-on-evict, rent letters, persistence) is abstracted
> behind interfaces/delegates and verified in tests with mocks + `EventAggregator` events. Real wiring
> happens in Phases 2–3.

## Phase 1 goal & definition of done

**Goal:** A self-contained `House` aggregate with value objects, an in-memory store contract, a
factory, and a complete unit-test suite — all compiling and green, plus the `domain-tests.md`
deliverable.

**Done when:**
- [ ] `src/Core/NeoServer.Domain/Houses/**` compiles with zero new EF/Lua references.
- [ ] `dotnet test tests/NeoServer.Domain.Tests` passes, all `Houses/` tests green.
- [ ] `docs/house-system/domain-tests.md` exists and matches the implemented cases 1:1.
- [ ] No changes to map loading, persistence, or Lua (those are Phase 2/3).

**Reference to mirror:** `src/Core/NeoServer.Domain/Guild/Guild.cs` (+ its store) — copy its shape for
events, `EventAggregator` usage, and immutability conventions.

---

## Pre-flight (verify before writing code)

These are the existing domain types Phase 1 depends on. **Confirm the exact interface names/members**
(grep first; the plan assumed these from exploration):

- [ ] `IPlayer` — `Guid`/`Id`, `AccountId`, `Name`, `Town`, `Bank` (`IBank.BankAmount`, a withdraw method), `Location`. Path: `src/Core/NeoServer.Domain/Common/Contracts/Creatures/IPlayer.cs`.
- [ ] `IDynamicTile` — has `uint? HouseId`, `Func<ICreature,bool> CanEnterFunction`, `HasFlag/SetFlag(TileFlags)`, `AllItems`, `Location`. Path: `src/Core/NeoServer.Domain/Common/Contracts/World/Tiles/IDynamicTile.cs`.
- [ ] `TileFlags.ProtectionZone` exists. Path: `src/Core/NeoServer.Domain/Common/Location/TileFlags.cs`.
- [ ] Door / Bed item contracts — **confirm exact names** (assumed `IDoorItem`, `IBedItem`). Grep `Door`, `Bed` under `src/Core/NeoServer.Domain/Items`. If a bed "sleeper/wakeup" API doesn't exist yet, model the wake step behind a small interface (see Step 5) and leave the concrete bed integration to Phase 2.
- [ ] `ICoinTypeStore` and the player bank withdraw signature used for rent.
- [ ] In-memory store base (`IDataStore<TKey,TValue>` / `DataStore<...>`) used by `GuildStore`. Path under `src/Database/NeoServer.Data.InMemory.DataStores/` (mirror it for `HouseStore`).
- [ ] `IGuildStore` lookup-by-name + a guild's rank-level-by-name accessor (for access-list resolvers). Path: `src/Core/NeoServer.Domain/...Guild`.
- [ ] Test helpers: `PlayerTestDataBuilder.Build(...)`, `EventAggregatorTestHelper.SetupEventAggregator<T>`. Path: `tests/NeoServer.Domain.Tests/Helpers/`.

> If any assumed name is wrong, adjust the signatures below — the **logic** is what matters, not the
> exact type names.

---

## Build order (dependency-first)

Each step lists **What → Where → Depends on → Done-when**. Do them in order; the aggregate is built
last from primitives so tests can target each piece in isolation.

### Step 1 — Scaffold the folder
- **What:** Create `Houses/` and `Houses/AccessList/` folders in the domain project. No new csproj.
- **Where:** `src/Core/NeoServer.Domain/Houses/`
- **Depends on:** nothing.
- **Done-when:** folders exist; project still builds.

### Step 2 — Enums & constants
- **What:**
  - `HouseAccessLevel.cs` → `enum HouseAccessLevel : byte { NotInvited=0, Guest=1, SubOwner=2, Owner=3 }` (compared with `>=`).
  - `HouseRentResult.cs` → `enum { NotDue, Paid, Warned, Evicted }`.
  - `HouseListId.cs` → `static class` with `const uint GuestList = 0x100; const uint SubOwnerList = 0x101;` and a helper `static bool IsDoorList(uint listId) => listId <= 254;`.
- **Where:** `Houses/`
- **Depends on:** Step 1.
- **Done-when:** compiles; trivially unit-testable.

### Step 3 — `HouseAccessList` value object
- **What:** `Houses/AccessList/HouseAccessList.cs` with structured API:
  `void AddPlayer(string name)`, `void AddGuild(ushort guildId)`,
  `void AddGuildRank(ushort guildId, byte rankLevel)`, `void AllowAll()`,
  `void Clear()`, `bool IsInList(IPlayer player)`.
  No text parsing — receives already-resolved data from `HouseAccessListLoader`
  (Loaders project, Phase 2). Case-insensitive player name matching.
- **Where:** `Houses/AccessList/`
- **Depends on:** Steps 1–2.
- **Done-when:** `HouseAccessListTests` (Step 12) pass against it in isolation.

### Step 4 — Domain events
- **What:** Mirror Guild events. Create under `Houses/Events/` (or the project's event location):
  - `HouseOwnerChangedEvent(House house, uint oldOwnerGuid, uint newOwnerGuid)`.
  - `HouseRentWarningEvent(House house, IPlayer owner, byte warningNumber)` (drives the rent letter in Phase 3).
  - `HouseEvictedEvent(House house)` (optional; or reuse owner-changed with new=0).
- **Where:** `Houses/Events/`
- **Depends on:** Step 1; the existing `EventAggregator` pattern.
- **Done-when:** events compile; raised via `EventAggregator.Invoke(...)` from the aggregate.

### Step 5 — Collaborator seams (for HouseService, not House aggregate)
- **What:** Interfaces consumed by `HouseService` for game-world side effects. `House` aggregate
  stays pure — no seam references. Phase 1 mocks these in `HouseService` tests.
  - `IHouseDepotTransfer` → `void TransferToOwnerDepot(int ownerAccountId, ushort townId, IEnumerable<IItem> items)`.
  - `IHouseEviction` → `void TeleportToExit(IPlayer player, Location exit)`.
  - `IHouseBedWaker` → `void WakeAll(IEnumerable<IItem> beds)`.
- **Where:** `Houses/Services/`
- **Depends on:** Steps 1, 4.
- **Done-when:** interfaces compile.

### Step 6 — `House` state + linkage
- **What:** `Houses/House.cs` (concrete class, no IHouse interface).
  - **State:** `Id`, `Name`, `TownId`, `Rent`, `PaidUntil`, `PayRentWarnings` (property setter clamps 0..7),
    `OwnerGuid` (0 = unowned), `OwnerName`, `OwnerAccountId`, `EntryPosition`;
    private `_tiles` (List/Dict), `_doors` (`Dictionary<uint, IDoorItem>`), `_beds` (List), `_accessLists` (`Dictionary<uint, HouseAccessList>`).
  - **Public read:** `Tiles`, `Doors`, `Beds`, `TileCount`, `DoorCount`, `BedCount` as read-only views.
  - **Linkage:** `LinkTile(IDynamicTile)` → add; `tile.SetFlag(TileFlags.ProtectionZone)`; install
    `tile.CanEnterFunction = c => c is IPlayer p && GetAccessLevel(p) != HouseAccessLevel.NotInvited`;
    throw if the tile already belongs to another house. `LinkDoor(uint doorId, IDoorItem)`; `LinkBed(IBedItem)`.
    `EntryPosition` defaults to the first linked tile if unset.
- **Where:** `Houses/`
- **Depends on:** Steps 2–5.
- **Done-when:** `HouseTileAssociationTests` + `HouseDoorBedTests` pass.

### Step 7 — Access resolution
- **What:** On `House`:
  - `HouseAccessLevel GetAccessLevel(IPlayer)` → Owner if `Group.Access` or `CanEditHouses`; else Owner if guid match (and owner≠0); else SubOwner if in subowner list and premium (when required); else Guest if in guest list; else NotInvited.
  - `bool IsInvited(IPlayer)` ⇒ `GetAccessLevel != NotInvited`.
  - `bool CanEnter(ICreature)` ⇒ non-players true; players ⇒ `IsInvited`.
  - `bool CanEditAccessList(uint listId, IPlayer)` ⇒ owner edits any list; subowner edits guest list only (not door lists).
  - `string GetAccessList(uint listId)` / `void SetAccessList(uint listId, HouseAccessList list)` (store structured list; door listId updates only that door's list).
- **Where:** `Houses/House.cs`
- **Depends on:** Steps 3, 6.
- **Done-when:** `HouseAccessLevelTests` pass.

### Step 8 — `SetNewOwner` (pure domain)
- **What:** `void SetNewOwner(uint guid, string name, int accountId, bool updatePaidUntil, DateTime now, uint rentPeriodSeconds)`
  porting `House::setOwner`. Pure state mutation — no game-world I/O.
  1. If currently owned **and** owner changes: clear all access + door lists.
  2. Set owner fields (0/empty = unowned).
  3. If `updatePaidUntil && guid != 0`: `PaidUntil = now + rentPeriod`; reset warnings to 0.
  - Same-guid call is a no-op except optional paidUntil refresh.
  - No eviction/wake/depot in aggregate. `HouseService` reads `Tiles`/`Players`/`Beds`/`AllItems`
    after calling this and performs side effects via seams.
- **Where:** `Houses/House.cs`
- **Depends on:** Steps 6–7.
- **Done-when:** `HouseOwnershipTests` pass (owner state changes, lists cleared).

### Step 9 — Kick / eviction (pure check)
- **What:** `bool CanKick(IPlayer caster, IPlayer target)` → returns true when the target is on a
  house tile, `GetAccessLevel(caster) >= GetAccessLevel(target)`, and the target does not have
  `CanEditHouses`. Pure check — no teleport inside aggregate. `HouseService` calls `IHouseEviction`
  on success.
- **Where:** `Houses/House.cs`
- **Depends on:** Steps 7.
- **Done-when:** `HouseEvictionTests` pass.

### Step 10 — `PayRent`
- **What:** `HouseRentResult PayRent(IPlayer owner, ICoinTypeStore coins, DateTime now, uint rentPeriodSeconds)`
  porting `payHouses`: rent 0 / unowned / not-yet-due ⇒ `NotDue`; due + sufficient bank ⇒ withdraw,
  advance `PaidUntil`, reset warnings ⇒ `Paid`; insufficient ⇒ increment warnings ⇒ `Warned`;
  on 7th warning ⇒ `SetNewOwner(0,...)` ⇒ `Evicted`. No events, no persistence inside aggregate.
  `HouseService` raises notification events and calls `IHouseRepository` after this returns.
- **Where:** `Houses/House.cs`
- **Depends on:** Steps 4, 8.
- **Done-when:** `HouseRentTests` pass.

### Step 11 — `HouseService` (domain service, mirrors `MailService`)
- **What:**
  - `Houses/Services/IHouseService.cs` + `HouseService.cs`.
  - Constructor: `IHouseRepository`, `IHouseEviction`, `IHouseBedWaker`, `IHouseDepotTransfer`.
  - `SetOwner(House, guid, name, accountId, updatePaidUntil, now, rentPeriodSeconds)` → calls
    `House.SetNewOwner(...)`, then iterates `House.Tiles` to evict now-uninvited players
    (`IHouseEviction`), wake beds (`IHouseBedWaker`), transfer pickupable items
    (`IHouseDepotTransfer`). Persists via `IHouseRepository`. Raises `HouseOwnerChangedEvent`.
  - `PayRent(House, owner)` → calls `House.PayRent(...)`, persists via `IHouseRepository`,
    raises `HouseRentWarningEvent` / `HouseEvictedEvent` based on result.
  - `KickPlayer(House, caster, target)` → calls `House.CanKick(...)`, on success teleports
    target via `IHouseEviction`. Does not persist: kick mutates no house state.
- **Where:** `Houses/Services/`
- **Depends on:** Steps 5, 8, 10, and `IHouseRepository` contract.
- **Done-when:** service compiles; ownership/rent/eviction DB flow verified via mocks.
- **Note:** `House` aggregate stays pure (no seam references). All game-world I/O happens here.

### Step 12 — Store + factory + movement policy contracts
- **What:**
  - `Common/Contracts/DataStores/IHouseStore.cs : IDataStore<uint, House>` + `GetByTile(ITile)` (reads `(tile as IDynamicTile)?.HouseId`) + `GetByHouseId(uint)`.
  - `HouseStore` impl in the in-memory data-store project, mirroring `GuildStore`.
  - `Houses/IHouseFactory.cs` + `HouseFactory` → `Create(HouseEntity)` creates aggregate with empty access lists. In Phase 2, `HouseAccessListLoader` populates lists from `HouseListEntity` rows after factory call.
  - `Houses/IHouseItemMovementPolicy.cs` → `bool CanMoveItem(IPlayer, ITile)` (consumed by the movement layer in Phase 2/3; ship the interface + a simple impl now so it's testable).
- **Where:** as listed.
- **Depends on:** Steps 6–10.
- **Done-when:** `HouseFactoryTests` + the store association tests pass.
- **Note:** `HouseFactory` references `HouseEntity` (a `NeoServer.Data` type) — confirm the domain
  already references that assembly the way `GuildLoader`/factory paths do; if not, the factory may
  belong in the loaders/data layer instead of `Domain`. **Resolve this boundary before coding Step 11.**

### Step 13 — Test data builder + unit tests
- **What:** `tests/NeoServer.Domain.Tests/Helpers/House/HouseTestDataBuilder.cs` (fluent builder:
  owner, rent, tiles, doors, beds, access lists, injected mock seams). Then the test classes under
  `tests/NeoServer.Domain.Tests/Houses/`, one per area (full case list in the deliverable below).
  Use `PlayerTestDataBuilder` and `EventAggregatorTestHelper.SetupEventAggregator<T>`; mock the Step-5 seams with Moq.
  For `HouseService`, mock `IHouseRepository` and verify persistence calls.
- **Where:** `tests/NeoServer.Domain.Tests/Houses/`
- **Depends on:** Steps 2–12.
- **Done-when:** all listed tests green.

### Step 14 — Deliverable: `domain-tests.md`
- **What:** Create `docs/house-system/domain-tests.md` enumerating every test with its **expected
  output** (see template below). Keep it 1:1 with Step-13 tests.
- **Where:** `docs/house-system/`
- **Depends on:** Step 13.
- **Done-when:** every implemented test appears with a matching expected-output line.

### Step 15 — Close-out
- [ ] `dotnet build` clean (no new warnings in `Houses/`).
- [ ] `dotnet test tests/NeoServer.Domain.Tests` green.
- [ ] Grep confirms no `Microsoft.EntityFrameworkCore` / Lua usings inside `Houses/` (except the factory's `HouseEntity` reference, if that boundary was accepted in Step 12).
- [ ] Open PR1 with the domain + tests + `domain-tests.md` only.

---

## Deliverable template — `docs/house-system/domain-tests.md`

Each row: **Test → Expected output**. Groups map to the Step-12 test classes.

```markdown
# House System — Domain Unit Tests (Expected Output)

## HouseOwnershipTests
- SetNewOwner_FromUnownedToPlayer_SetsOwnerGuidNameAccount → owner fields equal the supplied values.
- SetNewOwner_WithUpdatePaidUntilTrue_SetsPaidUntilToNowPlusRentPeriod → PaidUntil == now + rentPeriod.
- SetNewOwner_WithUpdatePaidUntilTrue_ResetsPayRentWarningsToZero → PayRentWarnings == 0.
- SetNewOwner_ChangingOwner_ClearsGuestAndSubownerLists → both lists empty.
- SetNewOwner_ChangingOwner_ClearsAllDoorAccessLists → every door list empty.
- SetNewOwner_ToZeroGuid_MarksHouseUnowned → OwnerGuid == 0 and lists empty.
- SetNewOwner_ToSameGuid_DoesNotClearLists → lists intact, no change to access/doors.
- Note: eviction/wake/depot side effects tested in HouseServiceTests.

## HouseAccessListTests
- AddPlayer_AddsNameCaseInsensitive → IsInList true regardless of case.
- AllowAll_MakesAnyPlayerInList → any player IsInList true.
- AddGuild_AddsGuildId → guild member IsInList true.
- AddGuildRank_MatchesRankLevelOrAbove → player with same/higher rank IsInList true.
- Clear_ResetsAllEntries → IsInList false for all previously added entries.
- IsInList_PlayerNotInList_ReturnsFalse → false.

## HouseAccessLevelTests
- GetAccessLevel_Owner_ReturnsOwner → Owner.
- GetAccessLevel_CanEditHouses_ReturnsOwner → Owner.
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
- CanEditAccessList_GuestEditsAnyList_ReturnsFalse → false.
- House_allows_owner_to_edit_door_list → true.
- House_denies_subowner_editing_door_list → false.

## HouseEvictionTests (pure aggregate checks)
- CanKick_OwnerKicksGuest_ReturnsTrue → returns true.
- CanKick_SubownerKicksGuest_ReturnsTrue → returns true.
- CanKick_GuestKicksOtherGuest_ReturnsTrue → true (equal access).
- CanKick_GuestKicksSelf_ReturnsTrue → true.
- CanKick_GuestKicksSubowner_ReturnsFalse → false (caster access too low).
- CanKick_SubownerKicksOwner_ReturnsFalse → false (cannot kick higher access).
- CanKick_CannotKickPlayerWithCanEditHouses_ReturnsFalse → false.
- CanKick_CanEditHousesKicksOwner_ReturnsTrue → true (staff counts as owner access).
- CanKick_TargetNotInHouse_ReturnsFalse → returns false.
- Note: teleport side effect tested in HouseServiceTests.

## HouseRentTests
- PayRent_NotDue_ReturnsNotDue_NoDeduction → NotDue, bank unchanged.
- PayRent_DueAndSufficientBank_DeductsRentAndAdvancesPaidUntil → Paid, bank -= rent, PaidUntil advanced.
- PayRent_DueAndSufficientBank_ResetsWarningsToZero → warnings == 0.
- PayRent_DueAndInsufficientBank_IncrementsWarnings → Warned, warnings += 1.
- PayRent_SeventhWarning_ReturnsEvicted → Evicted, OwnerGuid == 0.
- PayRent_RentZero_ReturnsNotDue → NotDue.
- PayRent_Unowned_ReturnsNotDue → NotDue.
- SetPayRentWarnings_AboveCap_ClampsToSeven → value == 7.
- Note: warning/eviction events raised by HouseService, not aggregate.

## HouseTileAssociationTests
- LinkTile_SetsProtectionZoneFlag → tile.HasFlag(ProtectionZone) true.
- LinkTile_InstallsCanEnterFunction_BlockingUninvitedPlayer → uninvited CanEnterFunction false.
- LinkTile_CanEnterFunction_AllowsInvitedPlayer → invited CanEnterFunction true.
- GetTileCount_AfterLinkingTiles_ReturnsCount → count matches linked tiles.
- Store_GetByTile_ReturnsOwningHouse → returns the linked house.
- Store_GetByTile_TileWithoutHouseId_ReturnsNull → null.
- LinkTile_SameTileToTwoHouses_Throws → throws.

## HouseDoorBedTests
- House_returns_door_id_when_position_matches_linked_door → expected door id.
- House_returns_null_when_no_door_at_position → null.
- SetAccessList_DoorListId_UpdatesThatDoorOnly → only that door's list changes.
- GetDoorCount_AfterLinking_ReturnsCount → count matches.
- LinkBed_AddsBedToHouse_GetBedCountReflects → count matches.
- SetNewOwner_WakesAllLinkedBeds → bed-waker seam called with all beds.

## HouseFactoryTests
- Create_FromEntity_MapsIdNameTownRentWarningsOwner → fields mapped from entity.
- Create_FromEntity_SeedsGuestListFromListEntity_0x100 → guest list populated.
- Create_FromEntity_SeedsSubownerListFromListEntity_0x101 → subowner list populated.
- Create_FromEntity_SeedsDoorAccessListFromDoorListId → door list populated.
- Create_UnownedEntity_ProducesUnownedHouseWithEmptyLists → OwnerGuid 0, lists empty.

## HouseServiceTests
- SetOwner_EvictsNowUninvitedPlayers → eviction seam called for each player on tiles who isn't new owner.
- SetOwner_WakesAllBeds → bed-waker seam called with all house beds.
- SetOwner_TransfersPickupableItemsToOldOwnerDepot → depot seam receives pickupable items from tiles.
- SetOwner_LeavesNonPickupableItems → depot seam not called with fixed/bed/door items.
- SetOwner_PersistsHouse → repository.Save called after aggregate updated.
- SetOwner_RaisesHouseOwnerChangedEvent → event published for player notification.
- PayRent_Warned_RaisesHouseRentWarningEvent → event published when PayRent returns Warned.
- PayRent_Evicted_RaisesHouseEvictedEvent → event published when PayRent returns Evicted.
- PayRent_Paid_PersistsHouseState → repository.Save called.
- CanKick_Success_TeleportsTargetWithoutPersisting → eviction seam called; no repository.Save.
- CanKick_Failure_NoSideEffects → no eviction or persist called.
```

---

## Risks specific to Phase 1
1. **Domain ↔ `HouseEntity` boundary** (Step 12) — if `Domain` shouldn't reference `NeoServer.Data`, move `HouseFactory` to the data/loaders layer and keep `Domain` clean. Decide before coding.
2. **Bed/Door item APIs** may not expose sleeper/wakeup or door-id today — fall back to the Step-5 seams and finish real integration in Phase 2.
3. **Clock determinism** — never read `DateTime.UtcNow` inside the aggregate; pass `now` in so rent/owner tests are deterministic.
4. **Depot transfer for offline owner** is only *simulated* via a mock in Phase 1; the real town-depot lookup lands in Phase 2.
5. **Repository interface in domain** — `IHouseRepository` lives in `NeoServer.Domain.Repositories` (same as `IPlayerMailRepository`). Confirm no EF dependency leaks through it.
```
