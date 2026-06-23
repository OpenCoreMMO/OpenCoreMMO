# House System Phase 1 — Code-Review Fix Spec

> Executable spec for resolving the code-review findings on commit `a1d37fd55`
> (branch `cvidal/house-system`). Companion to `House-System-Plan.md`,
> `house-phase1-steps.md`, and `domain-tests.md`.
>
> **Scope rule:** Phase 1 stays pure domain — no EF, no Lua, no map loading.
> The `House` aggregate stays pure (no I/O, clock passed in). Every change below
> respects those boundaries.
>
> **Hard definition of done (whole spec):**
> `dotnet test tests/NeoServer.Domain.Tests` runs **to completion** (no host
> crash / "Test Run Aborted") and is **green**. Today the run aborts with a
> StackOverflow — see C1.

All file/line references below were verified against the working tree at the time
of writing.

---

## Verified ground truth (read before editing)

- `src/Core/NeoServer.Domain/Common/Location/Structs/Location.cs`
  - `Equals(object obj)` at **line 403–406** is self-recursive: `obj is Location && Equals(obj)`
    re-dispatches to `Equals(object)` (static type of `obj` is `object`), not to
    `Equals(Location)`. Infinite recursion → StackOverflow (uncatchable, crashes host).
  - `operator ==` at **line 100–110** and `operator !=` at **line 112–122** each wrap a
    field comparison in `try { ... } catch (NullReferenceException) { return false; }`.
    `Location` is a `struct` — its fields are value types — so the body cannot throw
    `NullReferenceException`. The catch is dead code.
  - `Equals(Location obj)` at **line 90–93** delegates to `operator ==` and is correct.
  - This buggy `Equals(object)` shape exists **only** in `Location.cs` (grep confirmed).
- `src/Core/NeoServer.Domain/Houses/House.cs`
  - `LinkTile(IDynamicTile)` at **line 41–51**: adds tile, sets `CanEnterFunction`,
    defaults `EntryPosition`. Does **not** set `TileFlags.ProtectionZone`.
  - The only duplicate guard is `_tiles.Contains(tile)` at **line 43** — same-instance,
    same-house only. No cross-house detection.
  - `PayRent(IPlayer, ICoinTypeStore coinTypeStore, DateTime, uint)` at **line 159**:
    `coinTypeStore` is never read; rent is taken via `owner.Bank.Debit(Rent)` at line 167.
- `src/Core/NeoServer.Domain/Houses/HouseItemMovementPolicy.cs` **line 9–15**:
  `CanMoveItem` has dead null-guards and unconditionally `return true`.
- `src/Core/NeoServer.Domain/Houses/Services/HouseService.cs` **line 72–85** and
  `IHouseService.cs` **line 13**: `PayRent` carries the same unused `coinTypeStore`.
- Tile flag plumbing:
  - `IDynamicTile` (`...Common/Contracts/World/Tiles/IDynamicTile.cs`) has **no** `HouseId`
    and **no** flag-mutation member.
  - `ITile` (`...Common/Contracts/World/Tiles/ITile.cs`) exposes `bool HasFlag(TileFlags)`
    (line 33) and a read-only `bool ProtectionZone` getter (line 17).
  - `BaseTile` (`...World/Models/Tiles/BaseTile.cs`) has `public bool HasFlag(TileFlags)`
    (line 42) but `protected void SetFlag(TileFlags)` / `RemoveFlag` (line 80, 85).
  - `DynamicTile` (`...World/Models/Tiles/DynamicTile.cs`) holds `uint? HouseId { get; private set; }`
    (line 36). `HouseStore.GetByTile` already reads `(tile as DynamicTile)?.HouseId`
    (`src/Database/NeoServer.Data.InMemory.DataStores/HouseStore.cs:15`).
- `HouseTestDataBuilder.CreateTileMock` (`tests/.../Helpers/House/HouseTestDataBuilder.cs:75`)
  sets up `HasFlag` → `false` and `SetupProperty(CanEnterFunction)`; it does **not** set up
  any flag-setter. The builder is the seam to extend for M1/M2 tests.

---

## Execution order

C1 is a host-crash and blocks the entire suite — **do it first and confirm the full
suite completes before touching anything else.** Then M1, M2, m1, m2; finish with the
doc reconciliation (m3) and the Phase-3 note (m4).

| Phase | Issue | Title | Blocks |
|-------|-------|-------|--------|
| 1 | C1 | Fix `Location.Equals(object)` recursion (StackOverflow) | everything |
| 2 | M1 | Set `ProtectionZone` flag in `LinkTile` | — |
| 2 | M2 | Detect tile already owned by another house | — |
| 3 | m1 | Implement `HouseItemMovementPolicy` invited-check | — |
| 3 | m2 | Resolve unused `ICoinTypeStore` in rent path | — |
| 4 | m3 | Reconcile `domain-tests.md` with landed changes | C1, M1, M2 |
| 4 | m4 | `GetDoorIdByPosition` decision (Phase-3 dependency) | — |

---

## Phase 1 — C1: `Location.Equals(object)` infinite recursion (CRITICAL, blocks suite)

**What:** Fix the self-recursive `Equals(object)` override so boxed/object-path
equality (used by FluentAssertions `.Be()`, `Assert.Equal`, dictionary/set keys, etc.)
works instead of overflowing the stack. Clean the dead `NullReferenceException`
catches while in the file.

**Where:** `src/Core/NeoServer.Domain/Common/Location/Structs/Location.cs`
- `Equals(object)` line 403–406 (the bug).
- `operator ==` line 100–110 and `operator !=` line 112–122 (dead catches).

**Root cause:** `obj is Location && Equals(obj)` — the second operand binds to the
`Equals(object)` overload (static type of `obj` is still `object`), so the method calls
itself forever. The new test `HouseDoorBedTests.EntryPosition_DefaultsToFirstTileLocation`
is the first to box a `Location` through `.Be()`, which routes through `object.Equals`,
exposing the latent bug and aborting the run after ~54 tests.

**Fix:**
1. Replace the override body so it pattern-matches into the typed overload:
   ```csharp
   public override bool Equals(object obj)
   {
       return obj is Location other && Equals(other);
   }
   ```
   `Equals(other)` now binds to `Equals(Location)` (line 90), which uses `operator ==`.
2. Remove the dead `try/catch (NullReferenceException)` in `operator ==` (line 100–110)
   and `operator !=` (line 112–122); keep the plain field comparisons:
   ```csharp
   public static bool operator ==(Location origin, Location targetLocation)
       => origin.X == targetLocation.X && origin.Y == targetLocation.Y && origin.Z == targetLocation.Z;

   public static bool operator !=(Location origin, Location targetLocation)
       => !(origin == targetLocation);
   ```
   (Define `!=` in terms of `==` so the two operators cannot drift apart.)
3. Do not change `GetHashCode` (line 95–98) — it already hashes `X,Y,Z` consistently
   with the corrected equality.

**Confirm no code depends on the buggy behavior:**
- Grep for any caller that relied on `Location.Equals(object)` returning a wrong/recursive
  result — none can exist, since calling it crashed the process. The pre-fix `operator ==`
  semantics (field comparison) are unchanged by this fix, so equality results for code
  already using `==`/`!=`/`Equals(Location)` are identical; only the previously-fatal
  `object` path changes from "crash" to "correct".
- The `!=` rewrite changes the default-struct edge only if `==` and the old hand-written
  `!=` disagreed; they did not (both are field comparisons). No behavior change for
  callers.

**Test cases** (new file `tests/NeoServer.Domain.Tests/Common/Structs/LocationEqualityTests.cs`,
or append to existing `LocationTest.cs`):
- `Equals_BoxedEqualLocation_ReturnsTrue` → `((object)new Location(1,2,3)).Equals(new Location(1,2,3))` is `true` (and does not overflow).
- `Equals_BoxedDifferentLocation_ReturnsFalse` → boxed compare of `(1,2,3)` vs `(1,2,4)` is `false`.
- `Equals_BoxedNonLocation_ReturnsFalse` → `((object)new Location(1,2,3)).Equals("not a location")` is `false`.
- `Equals_TypedOverload_MatchesOperator` → `loc.Equals(other) == (loc == other)` for equal and unequal pairs.
- `Be_ViaFluentAssertions_DoesNotOverflow` → `new Location(1,2,3).Should().Be(new Location(1,2,3))` passes (regression guard for the exact crash path).
- `GetHashCode_EqualLocations_AreEqual` → equal locations share a hash code.
- `OperatorEquality_And_Inequality_AreConsistent` → for equal and unequal pairs, `(a == b) == !(a != b)`.

**Done-when:**
- The new equality tests pass.
- `dotnet test tests/NeoServer.Domain.Tests` runs **to completion** (no "Test Run Aborted")
  and is green. This is the gate for the whole spec — re-run the **full** suite, not just
  the `Houses` filter, after this fix.

---

## Phase 2 — M1: `LinkTile` must set `TileFlags.ProtectionZone` (MAJOR)

**What:** Per Plan decision #1 and Step 6, `House.LinkTile` must mark the tile a
protection zone at attach time. Restore the dropped acceptance test
`LinkTile_SetsProtectionZoneFlag`.

**Where:**
- `src/Core/NeoServer.Domain/Houses/House.cs` `LinkTile` (line 41–51).
- Tile contract/impl (see decision below).
- `tests/NeoServer.Domain.Tests/Houses/HouseTileAssociationTests.cs`.
- `tests/NeoServer.Domain.Tests/Helpers/House/HouseTestDataBuilder.cs` (`CreateTileMock`).

**Root cause:** `LinkTile` only sets `CanEnterFunction` + `EntryPosition`. The flag was
never wired because the aggregate sees only `IDynamicTile`, which has no flag-setter, and
`BaseTile.SetFlag` is `protected`.

**Decision — adopt option (a): expose a narrow flag-setting member on `IDynamicTile`.**
Rationale: the aggregate already mutates tile state through the interface
(`CanEnterFunction`), the world-attach (Phase 2) calls `LinkTile` on the same interface,
and the change is small and low-risk. Option (b) (defer to Phase 2 with a TODO) is the
fallback only if the team rejects widening the interface.

**Fix (option a):**
1. Add a minimal, explicit member to `IDynamicTile`
   (`...Common/Contracts/World/Tiles/IDynamicTile.cs`). Prefer a purpose-named method over
   exposing raw flag arithmetic, to keep the interface intent-revealing:
   ```csharp
   void SetAsProtectionZone();
   ```
   (Alternative if the team prefers symmetry with `HasFlag`: `void SetFlag(TileFlags flag);`
   — but that widens the surface more than needed. Pick one and use it consistently.)
2. Implement it on the concrete tile. `SetFlag` is already `protected` on `BaseTile`
   (line 80); add the public wrapper on `DynamicTile`
   (`...World/Models/Tiles/DynamicTile.cs`):
   ```csharp
   public void SetAsProtectionZone() => SetFlag(TileFlags.ProtectionZone);
   ```
   Do not change `BaseTile.SetFlag`'s `protected` visibility.
3. In `House.LinkTile`, after adding the tile and before/after installing
   `CanEnterFunction`, call `tile.SetAsProtectionZone();`. Keep it idempotent
   (`SetFlag` ORs the bit, so re-linking is harmless).
4. Update `HouseTestDataBuilder.CreateTileMock` so `SetAsProtectionZone` flips the
   mocked `HasFlag(TileFlags.ProtectionZone)` to return `true` after it is called
   (e.g. back the mock with a local bool the `HasFlag` setup reads, or
   `tileMock.Setup(x => x.SetAsProtectionZone()).Callback(...)`). Keep the existing
   default `HasFlag → false` for all other flags.

**Test cases** (`HouseTileAssociationTests`):
- `LinkTile_SetsProtectionZoneFlag` → after `LinkTile`, `tile.HasFlag(TileFlags.ProtectionZone)` is `true` (verify the member was invoked on the mock and the simulated flag reads true).
- Keep existing `LinkTile_InstallsCanEnterFunction_*` tests green (no regression).

**Done-when:**
- `LinkTile_SetsProtectionZoneFlag` passes.
- `domain-tests.md` re-lists the case under `HouseTileAssociationTests` (see m3).
- No EF/Lua reference added to `Houses/`.

---

## Phase 2 — M2: `LinkTile` must reject a tile already owned by another house (MAJOR)

**What:** Honor the plan rule "throw if a tile is linked to two houses." Real
cross-house detection, plus a correctly-named test that actually covers the cross-house
case.

**Where:**
- `src/Core/NeoServer.Domain/Houses/House.cs` `LinkTile` (line 41–51).
- `tests/NeoServer.Domain.Tests/Houses/HouseTileAssociationTests.cs` (`LinkTile_SameTileToTwoHouses_Throws`, line 60–69 — currently mis-named/mis-scoped).
- `tests/NeoServer.Domain.Tests/Helpers/House/HouseTestDataBuilder.cs` if a tile-owner seam is needed.

**Root cause:** `LinkTile` only checks `_tiles.Contains(tile)` (line 43), which detects
the **same** `House` instance re-linking the **same** tile. Two different `House` objects
can each link the same tile silently. The existing test links one tile to **one** house
twice, so it never exercises the cross-house path.

**Fix — detect ownership by a different house.** The aggregate sees only `IDynamicTile`,
which currently exposes no `HouseId`. Two viable detectors; pick **(A)** as primary:

- **(A) CanEnterFunction already installed.** `LinkTile` installs a `CanEnterFunction`
  closure on every tile it links and it is the only code path that does so for houses.
  So: if `tile.CanEnterFunction is not null` when a **different** house links it, that tile
  is already owned. Combine with the existing same-instance guard:
  ```csharp
  public void LinkTile(IDynamicTile tile)
  {
      if (_tiles.Contains(tile))
          throw new InvalidOperationException("Tile already belongs to this house.");

      if (tile.CanEnterFunction is not null)
          throw new InvalidOperationException("Tile already belongs to another house.");

      _tiles.Add(tile);
      tile.SetAsProtectionZone();              // M1
      tile.CanEnterFunction = c => c is IPlayer p && GetAccessLevel(p) != HouseAccessLevel.NotInvited;

      if (EntryPosition is null)
          EntryPosition = tile.Location;
  }
  ```
  This works without widening `IDynamicTile` for `HouseId`. Caveat to document: any
  non-house feature that sets `CanEnterFunction` on a tile would also trip this; grep
  confirms house linking is the writer in the domain, and the Phase-2 world-attach only
  calls `CanEnterFunction` via `LinkTile`. Note this assumption in code comment + plan.

- **(B) HouseId mismatch (stronger, needs interface widening).** Add `uint? HouseId { get; }`
  to `IDynamicTile` (it already exists on `DynamicTile` with a private setter, line 36) and
  throw when `tile.HouseId` is set to a **different** house id than `this.Id`. This is the
  most semantically precise check, but the OTBM tile carries its own `HouseId` from the map
  while `House.Id` comes from the DB; in Phase 1 these are decoupled, so id-equality cannot
  be relied on yet. **Defer (B) to Phase 2** when world-attach reconciles map `HouseId`
  with DB house id; record it as a TODO in the plan.

  > **Decision:** implement **(A)** now (no interface change, no EF). Leave a code comment
  > and a plan note that Phase-2 attach should upgrade to the `HouseId`-based check (B)
  > once map↔DB house ids are reconciled.

**Test changes** (`HouseTileAssociationTests`):
- **Rename/rescope** `LinkTile_SameTileToTwoHouses_Throws` to two distinct tests:
  - `LinkTile_SameTileToSameHouseTwice_Throws` → the existing same-instance behavior
    (link one tile to one house twice → `InvalidOperationException`). Keep coverage.
  - `LinkTile_SameTileToTwoDifferentHouses_Throws` → build two distinct `House` objects;
    `houseA.LinkTile(tile)` then `houseB.LinkTile(tile)` throws `InvalidOperationException`.
    Use a `CreateTileMock` whose `CanEnterFunction` is a real settable property
    (the builder already does `SetupProperty(x => x.CanEnterFunction)`, so once house A
    sets it the mock returns non-null for house B).

**Done-when:**
- Both cross-house and same-house tests pass.
- `domain-tests.md` reflects the two cases (see m3).
- No EF/Lua reference added to `Houses/`.

---

## Phase 3 — m1: Implement `HouseItemMovementPolicy` invited-check (MINOR)

**What:** Replace the no-op stub with the documented invited-only movement rule, using
the pieces that already exist (`IHouseStore.GetByTile`, `house.IsInvited`). Add tests.

**Where:**
- `src/Core/NeoServer.Domain/Houses/HouseItemMovementPolicy.cs` (line 9–15).
- `tests/NeoServer.Domain.Tests/Houses/` — new `HouseItemMovementPolicyTests.cs`.

**Root cause:** The stub unconditionally returns `true`; its XML doc promises invited-only
movement but no logic exists and there is no test.

**Fix:**
1. Inject `IHouseStore` into the policy:
   ```csharp
   public class HouseItemMovementPolicy(IHouseStore houseStore) : IHouseItemMovementPolicy
   {
       public bool CanMoveItem(IPlayer player, ITile tile)
       {
           if (tile is null) return true;            // no tile context → don't block

           var house = houseStore.GetByTile(tile);
           if (house is null) return true;           // not a house tile → unrestricted

           if (player is null) return false;         // house tile requires an acting player
           return house.IsInvited(player);
       }
   }
   ```
   - `IHouseStore` is a domain contract (`...Common/Contracts/DataStores/IHouseStore.cs`),
     so no EF leaks into `Houses/`.
   - `GetByTile` already returns `null` for non-house tiles (reads
     `(tile as DynamicTile)?.HouseId`), so the policy naturally no-ops off house tiles.
2. Update the XML doc to describe the real behavior (invited players may move items on
   house tiles; non-house tiles unrestricted; null player blocked on a house tile).
3. Confirm DI: register `IHouseItemMovementPolicy → HouseItemMovementPolicy` in the
   appropriate module if not already (Phase-1 may only ship the impl + interface; if the
   policy is not yet DI-registered, leave registration to the movement-layer wiring in
   Phase 2/3 and note it). Do **not** wire it into movement commands in Phase 1.

**Test cases** (`HouseItemMovementPolicyTests`, Moq `IHouseStore`):
- `CanMoveItem_NonHouseTile_ReturnsTrue` → `GetByTile` returns null → `true` for any player.
- `CanMoveItem_HouseTile_InvitedPlayer_ReturnsTrue` → store returns a house where `IsInvited(player)` is true → `true`.
- `CanMoveItem_HouseTile_UninvitedPlayer_ReturnsFalse` → store returns a house where `IsInvited` is false → `false`.
- `CanMoveItem_HouseTile_NullPlayer_ReturnsFalse` → house tile + null player → `false`.
- `CanMoveItem_NullTile_ReturnsTrue` → `true` (no tile context).

**Done-when:**
- All policy tests pass; the stub no longer unconditionally returns `true`.
- `domain-tests.md` gains a `HouseItemMovementPolicyTests` group (see m3).
- No EF/Lua reference in `Houses/`.

---

## Phase 3 — m2: Resolve the unused `ICoinTypeStore` in the rent path (MINOR)

**What:** Decide whether rent needs a coin source. It does not today — rent is debited
via `owner.Bank.Debit(Rent)`. Drop the unused parameter from both signatures, **or**
wire it if the team confirms Phase-3 rent must consume specific coin types.

**Where:**
- `src/Core/NeoServer.Domain/Houses/House.cs` `PayRent` (line 159, param unused; debit at line 167).
- `src/Core/NeoServer.Domain/Houses/Services/HouseService.cs` `PayRent` (line 72–85).
- `src/Core/NeoServer.Domain/Houses/Services/IHouseService.cs` (line 13).
- `tests/NeoServer.Domain.Tests/Houses/HouseRentTests.cs` and `HouseServiceTests.cs`
  (callers pass the arg today).

**Root cause:** `ICoinTypeStore coinTypeStore` was threaded through the rent API but the
implementation takes rent from the bank balance, so the parameter is dead.

**Decision — drop it (recommended).** The Phase-3 Lua/rent design (Plan §Phase 3,
`getPaidUntil`/`setPaidUntil`, bank-based rent) collects rent from the player's **bank
balance**, mirroring forgottenserver `payHouses`, with no coin-store lookup. Removing the
parameter now keeps the aggregate minimal and the signatures honest.

**Fix:**
1. Remove `ICoinTypeStore coinTypeStore` from:
   - `House.PayRent(IPlayer owner, DateTime now, uint rentPeriodSeconds)`.
   - `IHouseService.PayRent(House, IPlayer owner, DateTime now, uint rentPeriodSeconds)`.
   - `HouseService.PayRent(...)` (drop the field/forwarding).
2. Update all callers in the tests to the new signature.
3. Leave a one-line note in the plan (`House-System-Plan.md` Phase-1 PayRent bullet and
   Phase-3 rent section) that rent is bank-balance based and a coin-store was intentionally
   removed; if a future phase needs coin-specific rent, re-introduce it at the service
   layer, not the aggregate.

> **If the team instead wants coin-aware rent:** keep the parameter but actually use it in
> `House.PayRent` (resolve the coin type, debit from coins, fall back to bank), and add a
> `PayRent_UsesCoinTypeStore_*` test. Default to the drop unless explicitly told otherwise.

**Test cases:**
- Existing `HouseRentTests` / `HouseServiceTests` updated to the new signature — all
  previously-listed rent cases still pass unchanged in behavior.

**Done-when:**
- No `ICoinTypeStore` reference remains in the house rent path (grep clean).
- `dotnet test tests/NeoServer.Domain.Tests` green.

---

## Phase 4 — m3: Reconcile `domain-tests.md` with the landed changes (MINOR)

**What:** Bring `docs/house-system/domain-tests.md` back in step with the plan and with
whatever M1/M2/m1/m2/m4 decisions land. Annotate intentionally-deferred cases instead of
silently dropping them.

**Where:** `docs/house-system/domain-tests.md`.

**Root cause:** The committed doc was trimmed to match the code, dropping planned cases
(`LinkTile_SetsProtectionZoneFlag`, `GetDoorIdByPosition_*`, factory list-seeding,
`SetNewOwner_WakesAllLinkedBeds`) without marking which were legitimately moved (to
Phase 2 / `HouseServiceTests`) versus accidentally lost.

**Fix:** Edit `domain-tests.md` so it is 1:1 with the implemented Phase-1 tests after this
spec, and add an explicit **"Deferred to Phase 2/3"** sub-section listing moved cases with
their target:
- Add back under `HouseTileAssociationTests`:
  - `LinkTile_SetsProtectionZoneFlag` (M1, now implemented).
  - `LinkTile_SameTileToSameHouseTwice_Throws` and
    `LinkTile_SameTileToTwoDifferentHouses_Throws` (M2, replace the old mis-named entry).
- Add a `HouseItemMovementPolicyTests` group (m1) with its five cases.
- Update the `HouseRentTests` / `HouseServiceTests` rows if the `PayRent` signature changed
  (m2) — wording only, behavior identical.
- Add a **Deferred** block, e.g.:
  - `GetDoorIdByPosition_*` → deferred to Phase 3 (Lua `getDoorIdByPosition`); see m4.
  - factory door/list seeding (`Create_FromEntity_SeedsGuestList/SubownerList/DoorList_*`)
    → deferred to Phase 2 `HouseAccessListLoader` (factory creates empty lists in Phase 1).
  - `SetNewOwner_WakesAllLinkedBeds` → covered by `HouseServiceTests.SetOwner_WakesAllBeds`
    (the wake side effect lives in the service, not the aggregate) — cross-reference it.

**Done-when:**
- Every implemented Phase-1 test appears in `domain-tests.md` with an expected-output line.
- Every plan-listed case that is *not* implemented appears under a clearly-labelled
  Deferred section with its Phase-2/3 target. No silent omissions.

---

## Phase 4 — m4: `GetDoorIdByPosition` (MINOR, Phase-3 dependency)

**What:** Decide whether to add the position→door-id lookup now or formally defer it.

**Where:** `src/Core/NeoServer.Domain/Houses/House.cs` (doors keyed by door id in `_doors`,
line 14); plan reference Phase 3 Lua `getDoorIdByPosition`.

**Root cause:** Doors are stored as `Dictionary<uint doorId, IItem>` with no reverse
position index; the plan lists `GetDoorIdByPosition` but it was never implemented and the
test was dropped.

**Decision — defer to Phase 3, document the dependency.** A position→id lookup requires a
door item's tile/location, which is a world-attach concern (doors get their `Location` when
linked from the map in Phase 2). Implementing it meaningfully in pure Phase-1 would require
either storing a `Location` per door at link time or scanning items — premature before the
attach step exists.

**Fix (defer):**
1. In `House-System-Plan.md` (Phase 3, `HouseFunctions` `getDoorIdByPosition`) and in the
   m3 Deferred block, record that `GetDoorIdByPosition` is a **Phase-3 dependency** blocked
   on Phase-2 door-location linkage.
2. Optionally, if low-risk and desired now: have `LinkDoor(uint doorId, IItem door)` also
   capture the door's position into a `Dictionary<Location, uint>` reverse map and expose
   `uint GetDoorIdByPosition(Location position)` returning `0` when absent — **only** if the
   door item already carries a usable `Location` in Phase 1 (verify before committing; if
   not, defer per (1)). Add `GetDoorIdByPosition_KnownDoor_ReturnsDoorId` and
   `GetDoorIdByPosition_NoDoorAtPosition_ReturnsZero` tests if implemented.

> **Default:** defer (1). Only do (2) if a door's `Location` is reliably available in
> Phase-1 mocks/tests.

**Done-when:**
- The deferral (or implementation) is recorded in the plan and `domain-tests.md`.
- If implemented, the two lookup tests pass.

---

## Verification checklist (run in order)

1. **C1 gate (must pass before anything else is considered done):**
   `dotnet test tests/NeoServer.Domain.Tests`
   - Run completes — **no** "Test Run Aborted" / StackOverflow.
   - All tests green, including `LocationEqualityTests` and
     `HouseDoorBedTests.EntryPosition_DefaultsToFirstTileLocation`.
2. **Build:** `dotnet build` clean; no new warnings in `Houses/` or `Location.cs`.
3. **M1:** `HouseTileAssociationTests.LinkTile_SetsProtectionZoneFlag` green.
4. **M2:** `LinkTile_SameTileToSameHouseTwice_Throws` and
   `LinkTile_SameTileToTwoDifferentHouses_Throws` green.
5. **m1:** `HouseItemMovementPolicyTests` (5 cases) green.
6. **m2:** grep confirms no `ICoinTypeStore` in the house rent path; rent tests green.
7. **EF/Lua boundary:** grep `Houses/` for `Microsoft.EntityFrameworkCore` / Lua usings —
   none (factory's `HouseEntity` reference excepted per existing Phase-1 boundary).
8. **Docs:** `domain-tests.md` is 1:1 with implemented tests; Deferred section lists
   m4 + factory-seeding + bed-wake with targets.
9. **Full suite, final:** `dotnet test tests/NeoServer.Domain.Tests` green and complete.

## Risks

1. **C1 host-crash masking other failures.** Until C1 lands, the suite cannot report
   downstream results — do C1 first and re-run the full suite before evaluating M1/M2.
2. **M1 interface widening.** Adding `SetAsProtectionZone` to `IDynamicTile` touches a core
   contract; ensure all `IDynamicTile` mocks across the test suite still compile (Moq
   tolerates un-setup members, but compile-time the interface change is global). Grep for
   `Mock<IDynamicTile>` usages.
3. **M2 detector (A) false positives.** Relying on `CanEnterFunction != null` assumes house
   linking is the sole writer of that closure. Verified true in the domain today; document
   the assumption and upgrade to the `HouseId`-based check in Phase 2.
4. **m1 DI timing.** The policy gains an `IHouseStore` dependency; if registered now it must
   resolve `IHouseStore`. Keep movement-layer wiring out of Phase 1.
5. **m2 signature churn.** Dropping the parameter touches the aggregate, service, interface,
   and two test classes — update all in one change to keep the build green.
