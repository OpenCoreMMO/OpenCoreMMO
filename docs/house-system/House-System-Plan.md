# House System — Implementation Plan (OpenCoreMMO)

## Context

OpenCoreMMO has **partial, dormant house scaffolding** but no working house system:
`DynamicTile` already carries `uint? HouseId` and an injectable `CanEnterFunction`; `TileFactory`
already routes `houseId` into dynamic tiles; `WorldLoader.LoadTile` already reads `tileNode.HouseId`;
`NeoContext` already exposes `DbSet<HouseEntity>`/`DbSet<HouseListEntity>` with EF configurations.
What's missing is the **domain aggregate, the map-load attach step, the OTBM house-tile parser,
persistence/loader, the Lua API surface, and the Lua scripts** that turn those columns and that
`HouseId` into actual houses with ownership, access control, invited-entry, eviction and rent.

This plan ports the rules from `forgottenserver-downgrade` (`house.cpp`, `housetile.cpp`,
`luahouse.cpp`, and the house talkactions/spell scripts) onto OpenCoreMMO's idioms, mirroring the
**Guild** feature end-to-end (aggregate → store → repository → loader → Lua functions).

**Scope (confirmed):** Core ownership + rent — buy/sell/leave, access lists (guest/subowner/door),
invited-entry enforcement, eviction, rent cycle with 7-warning eviction. Auction/bidding
(`Bid/BidEnd/HighestBidder` columns) is **out of scope**, left intact for a future phase.

**Delivery (confirmed):** Three phased, independently-mergeable PRs (see end).

---

## Key design decisions

1. **No `HouseTile` subclass.** Realize the house tile by *configuring* the existing `DynamicTile`,
   not subclassing it. At attach time the `House` aggregate sets the `ProtectionZone` flag and
   installs a closure on `tile.CanEnterFunction` that consults the access list. Subclassing would
   fork the static/dynamic `TileFactory` routing, duplicate the large `DynamicTile` body, and break
   `is IDynamicTile` checks. House tiles are always dynamic (the factory already forces this for
   `houseId > 0`), so `tile as IDynamicTile` is always safe.

2. **Domain references no EF.** Persistence goes through `IHouseRepository` (domain interface)
   injected into `HouseService` (domain service), mirroring `MailService`/`IPlayerMailRepository`.
   The `House` aggregate stays pure; `HouseService` orchestrates operations that need DB access.
   Domain events still exist for player notifications (eviction, rent warning) but not for DB calls.

3. **Item throw/move gating lives in the movement layer, not in `DynamicTile`.** `DynamicTile.CanAddItem`
   has no acting-player context. Add a small, unit-testable `IHouseItemMovementPolicy` consulted by
   throw/move commands (`IHouseStore.GetByTile(toTile)` → `house.IsInvited(player)`).

4. **Mirror Guild** for every infrastructure piece so the PR reads idiomatically.

---

## Phase 1 — Domain + unit tests

New folder: `src/Core/NeoServer.Domain/Houses/`.

### Enums / value objects
- `HouseAccessLevel.cs` — `enum : byte { NotInvited=0, Guest=1, SubOwner=2, Owner=3 }` (compared with `>=`).
- `HouseListId.cs` — constants `Guest = 0x100`, `SubOwner = 0x101`; any value `0..254` is a **door id**.
- `AccessList/HouseAccessList.cs` — structured value object with
  `AddPlayer(name)`, `AddGuild(guildId)`, `AddGuildRank(guildId, minLevel)`, `AllowAll()`,
  `Clear()`, and `IsInList(IPlayer)`. No text parsing — receives already-resolved data
  from `HouseAccessListLoader` (in Loaders project, Phase 2).

### Aggregate `House.cs`
- **State:** `Id`, `Name`, `TownId`, `Rent`, `PaidUntil`, `PayRentWarnings` (clamped 0..7),
  `OwnerGuid` (0 = unowned), `OwnerName`, `OwnerAccountId`, `EntryPosition`,
  internal `_tiles`, `_doors` (keyed by door id), `_beds`, `_accessLists` (keyed by listId).
- **Linkage (called by loader after world load):**
  `LinkTile(IDynamicTile)` → adds tile, sets `ProtectionZone` flag, installs
  `CanEnterFunction = c => c is IPlayer p && GetAccessLevel(p) != NotInvited`;
  `LinkDoor(uint doorId, IDoorItem)`; `LinkBed(IBedItem)`. Throws if a tile is linked to two houses.
- **Access:** `GetAccessLevel(IPlayer)`, `IsInvited`, `CanEnter(ICreature)`,
  `CanEditAccessList(listId, IPlayer)` (owner edits subowner list; owner+subowner edit guest list/doors),
  `GetAccessList(listId)` / `SetAccessList(listId, HouseAccessList)`.
- **`SetNewOwner(guid, name, accountId, updatePaidUntil, now, rentPeriodSeconds)`** (ports `House::setOwner`):
  pure domain state change. On owner *change* → clear all access + door lists; set owner fields
  (0/empty = unowned); if `updatePaidUntil && guid != 0` set `PaidUntil = now + rentPeriod` and
  reset warnings. Same-guid = no-op except paidUntil. No eviction/wake/depot — `HouseService`
  reads `Tiles`/`Players`/`Beds`/`AllItems` after calling this and performs side effects.
- **`CanKick(caster, target)`** — pure check: returns `bool` (caster `>= SubOwner` and
  `level(caster) > level(target)`, never the owner, target in this house).
  `HouseService` teleports target on success.
- **`PayRent(owner, ICoinTypeStore, now, rentPeriodSeconds)`** (ports `payHouses`): returns
  `HouseRentResult { NotDue, Paid, Warned, Evicted }`. Not due / rent 0 / unowned → `NotDue`.
  Sufficient bank → withdraw, advance `PaidUntil`, reset warnings → `Paid`. Insufficient → increment
  warnings; on 7th → returns `Evicted`. No I/O inside the aggregate. `HouseService` raises events and persists.

### Domain events
- `HouseOwnerChangedEvent(House, oldOwnerGuid, newOwnerGuid)` — raised by `House.SetNewOwner`.
- `HouseRentWarningEvent(House, owner, warningNumber)` — raised by `HouseService` (not aggregate)
  to notify the player via game event.
- `HouseEvictedEvent(House)` — raised by `HouseService` when eviction occurs.

Events are for player notifications, not for DB persistence. `HouseService` calls `IHouseRepository` directly.

### HouseService (domain service, mirrors MailService)
- `Houses/Services/IHouseService.cs` + `HouseService.cs`. Constructor takes
  `IHouseRepository`, `IHouseEviction`, `IHouseBedWaker`, `IHouseDepotTransfer`.
- `SetOwner(House, guid, name, accountId, updatePaidUntil, now, rentPeriodSeconds)` — calls
  `House.SetNewOwner(...)`, then iterates `House.Tiles` to evict now-uninvited players (via
  `IHouseEviction`), wake beds (`IHouseBedWaker`), transfer pickupable items to old owner depot
  (`IHouseDepotTransfer`). Persists via `IHouseRepository`. Raises `HouseOwnerChangedEvent`.
- `PayRent(House, owner)` — calls `House.PayRent(...)`, persists via `IHouseRepository`,
  raises `HouseRentWarningEvent` or `HouseEvictedEvent` as needed.
- `KickPlayer(House, caster, target)` — calls `House.KickPlayer(...)`, on success teleports target
  via `IHouseEviction` and persists via `IHouseRepository`.

### Stores / factory
- `Common/Contracts/DataStores/IHouseStore.cs` : `IDataStore<uint, House>` + `GetByTile(ITile)`
  (reads `(tile as IDynamicTile)?.HouseId`) + `GetByHouseId(uint)`. Impl `HouseStore` in the
  in-memory data-store project, mirroring `GuildStore`.
- `Houses/IHouseFactory.cs` + impl — `Create(HouseEntity)` creates aggregate with empty access lists.
  In Phase 2, `HouseAccessListLoader` populates lists from `HouseListEntity` rows after factory call.
- `Houses/IHouseItemMovementPolicy.cs` — `CanMoveItem(IPlayer, ITile)` for the movement layer (Phase 2/3 hook).

### Deliverable: domain-test spec markdown
Create **`docs/house-system/domain-tests.md`** capturing every domain test case below with a
one-line *expected output*, grouped by area. This is the requested "domain unit tests output
expected" artifact and doubles as the Phase-1 acceptance checklist.

### Tests — `tests/NeoServer.Domain.Tests/Houses/` (xUnit + FluentAssertions + Moq + AutoFixture)
Add `Helpers/House/HouseTestDataBuilder.cs`; reuse `PlayerTestDataBuilder` and
`EventAggregatorTestHelper.SetupEventAggregator<T>`. Naming `[Method]_[Scenario]_[Expected]`.

- **HouseOwnershipTests:** set owner populates fields; updatePaidUntil sets future PaidUntil + resets
  warnings; owner change clears guest/subowner/door lists, evicts occupants to entry, wakes bed
  sleepers, transfers pickupables to old owner depot, leaves fixed items; zero-guid → unowned;
  same-guid → no evict/clear; raises `HouseOwnerChangedEvent` once.
- **HouseAccessListTests:** `AddPlayer` (case-insensitive), `AddGuild`, `AddGuildRank` (rank `>=` match),
  `AllowAll`, `Clear` resets state, `IsInList` combinations.
- **HouseAccessLevelTests:** owner/subowner/guest/none resolution; both-lists → SubOwner wins;
  `IsInvited` true/false; `CanEnter` invited/uninvited; non-player → true; `CanEditAccessList`
  owner-vs-subowner-list, subowner-vs-guest-list, guest-edits-anything-false.
- **HouseEvictionTests:** owner/subowner kicks guest → exit; guest kicks → fail; kick owner → fail;
  target not in house → fail; `KickOccupants` only moves now-uninvited.
- **HouseRentTests:** not due → NotDue/no deduction; due+funds → deduct + advance + reset warnings;
  due+no funds → increment warnings + letter event; 7th warning → evict + unown; rent 0 / unowned →
  NotDue; warnings above cap clamp to 7.
- **HouseTileAssociationTests:** `LinkTile` sets ProtectionZone; CanEnterFunction blocks uninvited /
  allows invited; tile count; `Store.GetByTile` returns owner house / null when no houseId; same tile
  to two houses throws.
- **HouseDoorBedTests:** `GetDoorIdByPosition` known/none; door-listId updates only that door; door
  count; `LinkBed` reflected in bed count; owner change wakes all beds.
- **HouseFactoryTests:** entity → id/name/town/rent/warnings/owner mapping; seeds guest (0x100),
  subowner (0x101), and door access lists; unowned entity → empty lists.

---

## Phase 2 — Map load + persistence

### OTBM house-tile parsing
Finish the stub `src/Loaders/NeoServer.Loaders/OTBM/Structure/TileArea/HouseTile.cs`: parse the
`OTBM_HOUSETILE (0x0E)` node — read `HouseId = stream.ReadUInt32()` **before** tile attributes, then
parse contents like a normal tile; surface `NodeType.HouseTile` and `HouseId` on `TileNode`.
**Highest-risk item:** a mis-aligned read corrupts the rest of the tile area — verify against a known map.

### Load ordering & attach
1. **Guilds load first** (so guild names resolve for access-list parsing).
2. **`HouseLoader`** (`src/Loaders/NeoServer.Loaders/Houses/HouseLoader.cs`, mirrors `GuildLoader`):
   `repo.GetAll()` → `factory.Create(...)` → `houseStore.AddOrUpdate`. Houses now have ownership but empty access lists.
3. **`HouseAccessListLoader`** (`src/Loaders/NeoServer.Loaders/Houses/HouseAccessListLoader.cs`):
   iterates `houseListEntities` per house, parses raw text, resolves guild names via `IGuildStore`,
   builds `HouseAccessList` via structured API (`AddPlayer`/`AddGuild`/`AddGuildRank`/`AllowAll`),
   and calls `house.SetAccessList(listId, list)`. Runs after `HouseLoader`.
4. **`WorldLoader`** gains `IHouseStore`. In `LoadTile`, for `houseId > 0`:
   `houseStore.GetByHouseId(id)?.LinkTile(dynamicTile)`, and scan tile items → `LinkDoor` (door + DoorId
   attr) / `LinkBed` (bed). **Skip the static-cache fast-path when `houseId > 0`** (house tiles must
   never be served from the shared cache).
5. **Finalize:** set each house `EntryPosition` (entity value, else first tile); log houses with 0 tiles.

### Persistence (mirror MailService pattern)
- `HouseEntity.cs`: rename `Onwer` → `OwnerGuid`; repurpose/rename `Paid` → `PaidUntil` (unix);
  add `EntryX/EntryY/EntryZ` (optional). Keep auction columns untouched.
- `HouseListEntity`: `ListId` semantics — `0x100` guest, `0x101` subowner, `0..254` door id; ensure
  `(HouseId, ListId)` index in `HouseListEntityConfiguration`.
- `IHouseRepository` + `HouseRepository : BaseRepository<HouseEntity>` (mirror `IPlayerMailRepository`):
  `GetAll()` / `GetById()` with `Include(h => h.HouseLists)`; `Save(House)`; `SaveAccessList(houseId, listId, text)`.
  Called directly by `HouseService`, not through domain events or delegates.
- **EF migration** for the rename + new columns + index, generated for SQLite and Postgres
  (InMemory ignores migrations).

---

## Phase 3 — Lua handlers + scripts

### `HouseFunctions.cs`
`src/Extensions/NeoServer.Scripts.LuaJIT/Functions/HouseFunctions.cs` (+ `IHouseFunctions`), mirroring
`GuildFunctions`/`TileFunctions`. `Init` registers class `House` (`House(id)` → `houseStore.GetByHouseId`),
`__eq`, and methods: `getId`, `getName`, `getTown`, `getExitPosition`, `getOwnerName`, `getOwnerGuid`,
`getOwnerAccountId`, `setOwnerGuid`, `getRent`/`setRent`, `getPaidUntil`/`setPaidUntil`,
`getPayRentWarnings`/`setPayRentWarnings`, `getTiles`/`getTileCount`, `getDoors`/`getDoorCount`,
`getDoorIdByPosition`, `getBeds`/`getBedCount`, `getItems`, `canEditAccessList(listId, player)`,
`getAccessList(listId)`, `setAccessList(listId, text)`, `startTrade(player, partner)`,
`kickPlayer(caster, target)`, `save`.

### Extend existing function classes (don't add new Tile/Player classes)
- `TileFunctions.Init`: add `Tile:getHouse()` → `houseStore.GetByTile(tile)`.
- `PlayerFunctions.Init`: add `Player:getTile()` → `gameServer.Map.GetTile(player.Location)` (reuse if present).

### Registration
- `IoC/LuaJITInjection.cs`: `AddSingleton<IHouseFunctions, HouseFunctions>()`.
- `LuaStartup`: add `IHouseFunctions` ctor param + `houseFunctions.Init(luaState)` in `Start()`.

### Ported Lua scripts (loaded recursively from `data/`)
- `data/scripts/talkactions/house/buyhouse.lua`, `leavehouse.lua`, `sellhouse.lua`
  (use `player:getTile()`, `tile:getHouse()`, `house:setOwnerGuid`, `house:startTrade`).
- `data/spells/scripts/house/edit_door.lua`, `invite_guests.lua`, `invite_subowners.lua`,
  `kick_guest.lua` (use `house:canEditAccessList`, `getAccessList`/`setAccessList`, `kickPlayer`,
  `getDoorIdByPosition`).

---

## DI wiring (exact modules)
- `DataStoreInjection.cs` → `IHouseStore, HouseStore`
- `DatabaseInjection.cs` → `IHouseRepository, HouseRepository`
- `FactoryInjection.cs` → `IHouseFactory, HouseFactory`
- `ServiceInjection.cs` → `IHouseService, HouseService`
- `LuaJITInjection.cs` → `IHouseFunctions, HouseFunctions`
- Loader orchestration → register `HouseLoader`; order **guilds → houses → world+attach**
- `WorldLoader` registration → add `IHouseStore` to its ctor

## Critical files
- `src/Core/NeoServer.Domain/World/Models/Tiles/DynamicTile.cs` (read; `HouseId`, `CanEnterFunction`)
- `src/Loaders/NeoServer.Loaders/World/WorldLoader.cs` (attach + cache-skip)
- `src/Loaders/NeoServer.Loaders/OTBM/Structure/TileArea/HouseTile.cs` (finish parser)
- `src/Database/NeoServer.Data/Entities/HouseEntity.cs` + `HouseListEntity.cs` (+ configs)
- `src/Extensions/NeoServer.Scripts.LuaJIT/Functions/{TileFunctions,PlayerFunctions}.cs`
- Reference aggregate to mirror: `src/Core/NeoServer.Domain/Guild/Guild.cs` (+ GuildRepository/GuildLoader/GuildStore)

## Risks
1. OTBM stream alignment when reading the house-tile uint32 (corruption risk) — verify on a real map.
2. Static-tile cache must be bypassed for `houseId > 0` in both `WorldLoader` and `TileFactory`.
3. Offline-owner depot transfer needs depot-store access, not online inventory.
4. Item throw/move gating must be a movement-layer policy (no player context in `DynamicTile`).
5. Guilds must load before houses for access-list guild/rank resolution.
6. Domain must stay EF-free (events/delegates for save + letters).
7. Migration spans SQLite + Postgres (InMemory ignores).

## Verification
- **Phase 1:** `dotnet test tests/NeoServer.Domain.Tests` — all `Houses/` tests green;
  `docs/house-system/domain-tests.md` matches implemented cases.
- **Phase 2:** boot the server with a house-bearing OTBM; assert houses in `IHouseStore` have the
  expected tile counts, ProtectionZone flags set, and owners loaded from DB; confirm migration applies
  on SQLite + Postgres.
- **Phase 3:** in-game — uninvited player blocked from a house tile (redirected to entry); owner runs
  buy/leave/sell talkactions; `aleta sio` / invite + kick spells update access lists and evict;
  rent cycle deducts bank and evicts after 7 warnings.

## Phasing (3 PRs)
- **PR1 (Phase 1):** domain aggregate, value objects, store/factory, unit tests, `domain-tests.md`. No infra deps.
- **PR2 (Phase 2):** OTBM parser, `HouseLoader`, `WorldLoader` attach, persistence/repository, migration, DI.
- **PR3 (Phase 3):** `HouseFunctions`, Tile/Player Lua additions, ported talkactions + spell scripts, DI.
