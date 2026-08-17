# House System — `!sellhouse` Talkaction

Companion to `house-phase3-steps.md`. Ports TFS `data/talkactions/scripts/sellhouse.lua` +
`House::startTrade` / `HouseTransferItem` onto OpenCoreMMO.

This is **not** CipSoft’s website house transfer (domain.md UC-08..UC-10: scheduled date,
bank debit, items stay). TFS `!sellhouse` is an in-game **safe-trade of a phantom transfer
document**. Completing the trade calls `setOwner(buyer)` immediately.

---

## TFS behavior (source of truth)

### Talkaction (`sellhouse.lua`)

Words: `!sellhouse` with `separator=" "`. Always swallows speech.

1. Resolve `Player(param)`. Missing, offline, or self → `"Trade player not found."`
2. House from **seller’s current tile** (`player:getTile():getHouse()`). Not inside a house →
   `"You must stand in your house to initiate the trade."`
3. `house:startTrade(player, tradePartner)`. If not `RETURNVALUE_NOERROR`, send that cancel.

Standing in the house is only required to **start**. Walking out after the window opens does
not cancel by itself (distance to partner still can).

### `house:startTrade` (`luahouse.cpp`)

Checked **in this order**, each returns a `RETURNVALUE_*` (Lua cancel uses `Game.getReturnMessage`):

| Check | Return | Message |
|---|---|---|
| Partner not in range (Chebyshev ≤ 2, **same floor**) | `TRADEPLAYERFARAWAY` | Trade player is too far away. |
| `house.owner != seller.guid` | `YOUDONTOWNTHISHOUSE` | You don't own this house. |
| Partner already owns a house | `TRADEPLAYERALREADYOWNSAHOUSE` | Trade player already owns a house. |
| Partner is highest bidder on an auctioned house | `TRADEPLAYERHIGHESTBIDDER` | Trade player is currently the highest bidder of an auctioned house. |
| Transfer already pending (`getTransferItem()` null) | `YOUCANNOTTRADETHISHOUSE` | You can not trade this house. |
| `internalStartTrade` fails (already trading, …) | still `NOERROR` | Trade layer already sent its own cancel; transfer item is reset. |

TFS does **not** check premium, level, or sight for `!sellhouse`. Do not add
`canOwnHouse` here (that gate is for `!buyhouse`). Auction bidding is out of scope —
treat “highest bidder” as always false until auctions exist.

### Transfer document

- Created as `ITEM_DOCUMENT_RO` (1968) with special description
  `It is a house transfer document for '{houseName}'.`
- Lives in a dummy locker, **not** on the map or in inventory. Parent is forced to the seller
  so the trade window accepts it.
- Only one pending transfer per house. Second `!sellhouse` while the window is open →
  `YOUCANNOTTRADETHISHOUSE`.
- Client trade UX is the normal 0x7D/0x7E window. Buyer must counter-offer an item (gold,
  etc.). That item **is** the payment; the server never prices the house.

### On accept (`HouseTransferItem::onTradeEvent(ON_TRADE_TRANSFER)`)

1. `House::executeTransfer` → `setOwner(buyerGuid)` then drop the transfer pointer.
2. Document is destroyed (never remains in the buyer’s backpack).
3. TFS `setOwner` when the house already has an owner:
   - Pickupables → old owner depot
   - Occupants kicked to exit
   - Beds woken
   - Guest / subowner / door lists cleared
   - **`paidUntil` kept** (buyer inherits remaining rent)
   - **`rentWarnings` reset to 0**
   - New owner assigned

### On cancel (`ON_TRADE_CANCEL`)

`resetTransferItem()`: destroy the phantom document, clear the pending pointer. Ownership
unchanged.

---

## OpenCoreMMO alignment notes

- Talkaction `return true` swallows chat (opposite of TFS booleans). Same as `!buyhouse`.
- Reuse `HouseService.SetOwner` for accept side-effects (evict / wake / depot / persist /
  `HouseOwnerChangedEvent`). Call it with **`updatePaidUntil: false`**.
- `SetNewOwner` today only zeroes `PayRentWarnings` when `updatePaidUntil` is true. TFS
  always zeroes warnings on a real owner change. Fix that in this slice (see Step 1).
- `ITradeService` currently only exposes `Cancel`. `SafeTradeSystem.Request` already exists
  and is what `TradeRequestCommand` uses.
- `TradeRequestValidation` rejects items that are not next to the player, not owned by the
  seller, or without line of sight. The phantom document fails those checks unless the
  trade path special-cases `IHouseTransferItem`.
- `TradeItemExchanger` always `Remove` + `AddItem` both sides. Putting the document on the
  seller’s **ground** location would make `ItemRemoveService` try to pull it off the tile
  the seller is standing on. Never place the document on the map.

---

## Design

Keep Lua as thin as TFS. All rules live in C#.

```
!sellhouse Name
  → sellhouse.lua
  → house:startTrade(seller, partner)
  → HouseTradeService.StartTrade
       validates (range / owner / partner house / pending)
       creates HouseTransferItem (id 1968, not on map)
       house.PendingTransfer = item
       SafeTradeSystem.Request(seller, partner, item)
       on Request failure → ResetTransfer, still NoError
  → trade window

both accept
  → TradeItemExchanger
       skip physical move of IHouseTransferItem
       move partner’s offered item as usual
       item.Complete(buyer) → HouseService.SetOwner(..., updatePaidUntil: false)

cancel / logout / walk too far
  → SafeTradeSystem.Cancel
       item.Cancel() → house.PendingTransfer = null
```

`HouseTradeService` (new) owns trade orchestration so `HouseService` does not take a
trade/item-factory dependency. `House` only holds the pending-transfer pointer
(same role as TFS `transferItem`).

---

## Implementation steps

### 1. Domain — pending transfer + warning reset

**`House`**

- `IHouseTransferItem PendingTransfer { get; private set; }`
- `bool TryAttachTransfer(IHouseTransferItem item)` — false if one is already attached
  (TFS `getTransferItem()` returning null).
- `void ResetTransfer()` — clear pointer only (item teardown is the service’s job).
- `bool ExecuteTransfer(IHouseTransferItem item, uint newOwnerGuid)` — true only when
  `item` is the attached pending item; then clear the pointer. Ownership change stays in
  `HouseService.SetOwner`.

**`SetNewOwner`**

When the owner actually changes (`guid != OwnerGuid`), always `PayRentWarnings = 0`.
Still only refresh `PaidUntil` when `updatePaidUntil && guid != 0`.

### 2. Domain — `IHouseTransferItem` + `HouseTransferItem`

New types under `src/Core/NeoServer.Domain/Houses/`:

```csharp
public interface IHouseTransferItem : IItem
{
    House House { get; }
    void Complete(IPlayer newOwner);
    void Cancel();
}
```

`HouseTransferItem` subclasses `Paper` (item 1968 is paper). Constructed by
`HouseTradeService` with `IItemTypeStore` (do **not** route through `ItemFactory` — that
would yield a plain `Paper` with no house link).

- `SetOwner(seller)` so `TradeRequestValidation.PlayerCannotTradeItem` passes.
- Location type **Container or Slot**, never Ground (so `ItemRemoveService` / `IsNextTo`
  do not treat it as a floor item). Container/Slot makes `Location.IsNextTo` return true.
- `Attributes.Description` =
  `It is a house transfer document for '{house.Name}'.`
- Override `GetLookText` to append that sentence. `InspectionTextBuilder` only reads
  `Metadata.Description`, so an override is required for look-in-trade / OTCv8 tooltips.
- `Complete` / `Cancel` are delegates set by `HouseTradeService` at creation (item must
  not depend on `HouseService` or `ITradeService`).

### 3. Domain — `HouseTradeService`

`Houses/Services/IHouseTradeService.cs` + `HouseTradeService.cs`.

Inject: `IHouseService`, `IHouseStore`, `SafeTradeSystem` (or extend `ITradeService` with
`Request` — prefer extending the interface so Lua/DI stay on contracts), `IItemTypeStore`,
`HouseConfiguration`.

`StartTrade(House house, IPlayer seller, IPlayer partner) → HouseTradeResult`

1. Null / same player → Lua already handled; still guard in C#.
2. `seller.Location.Z != partner.Location.Z` **or** Chebyshev distance > 2 → `TradePlayerFarAway`.
   (`GetMaxSqmDistance` ignores Z; TFS `isInRange(..., 2, 2, 0)` does not.)
3. `house.OwnerGuid != seller.Id` → `YouDontOwnThisHouse`.
4. `houseStore.GetByOwnerGuid(partner.Id) is not null` → `TradePlayerAlreadyOwnsAHouse`.
5. Auction stub: skip (always pass).
6. Create `HouseTransferItem` (type 1968). `TryAttachTransfer` false → `YouCannotTradeThisHouse`.
7. `tradeService.Request(seller, partner, item)`.
   - Failure → `ResetTransfer()`, return `NoError` (TFS: trade already cancelled the player).
   - Success → `NoError`.

`CompleteTransfer(House house, IHouseTransferItem item, IPlayer buyer)`

- `house.ExecuteTransfer(item, buyer.Id)` must succeed (stale item → no-op).
- `houseService.SetOwner(house, buyer.Id, buyer.Name, (int)buyer.AccountId,
  updatePaidUntil: false, DateTime.UtcNow, rentPeriodSeconds)`.

`CancelTransfer(House house)` → `house.ResetTransfer()`.

`HouseTradeResult` lives in Domain (do not leak Lua `ReturnValueType`):

`NoError`, `TradePlayerFarAway`, `YouDontOwnThisHouse`, `TradePlayerAlreadyOwnsAHouse`,
`TradePlayerHighestBidder`, `YouCannotTradeThisHouse`.

Register in `ServiceInjection.cs`.

### 4. SafeTrade hooks

**`ITradeService`** — add `SafeTradeError Request(IPlayer player, IPlayer secondPlayer, IItem item)`
so `HouseTradeService` does not depend on the concrete `SafeTradeSystem` if that stays
practical. `TradeRequestCommand` can switch to the interface too (optional cleanup).

**`TradeRequestValidation`** — when `items[0] is IHouseTransferItem`:

- Skip `HasNoSightClearToPlayer` (TFS `startTrade` has no sight check).
- Skip `PlayerIsNotNextToItem` (document is not on the map). Keep the **player–player**
  range check as a backstop; `StartTrade` already enforced TFS 2/2/0.

**`TradeItemExchanger.Exchange`**

- Identify which of the two root items is `IHouseTransferItem` (normally the seller’s).
- Physically exchange only the **other** item (buyer’s gold/item → seller).
- Do **not** `Remove`/`AddItem` the transfer document.
- Skip capacity/room checks **for adding the document to the buyer** (it is never added).
  Still validate the seller can receive the buyer’s offer.
- On success call `transferItem.Complete(buyer)` where buyer is the player who did **not**
  offer the document.
- If both sides somehow offered a transfer item, fail the trade (should be impossible).

**`SafeTradeSystem.Close` / `Cancel`**

Before untracking items, if any traded item is `IHouseTransferItem`, call `Cancel()`.
Must run on every close path (reject, logout, walk > 2 sqm from partner, item deleted).

Do not subscribe the phantom document to `OnDeleted`/`OnRemoved` in a way that
recursively cancels during `Complete` (Complete already detaches first).

### 5. Lua

**`HouseFunctions.LuaHouseStartTrade`**

```
house:startTrade(player, tradePartner) → RETURNVALUE_*
```

Map `HouseTradeResult` → `ReturnValueType` integers already registered
(`TRADEPLAYERFARAWAY` … `YOUCANNOTTRADETHISHOUSE`). `ReturnMessageExtensions` already has
the TFS strings.

Push `nil` when house/player/partner userdata is missing (TFS).

**`data/scripts/talkactions/house/sellhouse.lua`**

Port TFS script; OpenCoreMMO talkaction conventions:

- `TalkAction("!sellhouse")` + `separator(" ")`
- `Player(param)` for online lookup (existing `LuaPlayerCreate`)
- `player:getTile()` / `tile:getHouse()` (already bound)
- `sendCancelMessage` string or `RETURNVALUE_*` (existing `player.lua` helper)
- **Always `return true`** so the command is not spoken

### 6. Docs tracker

In `house-phase3-steps.md`, mark Slice 5 `!sellhouse` as the next talkaction slice and
leave `!leavehouse` in later slices.

---

## Tests

Follow `.agents/skills/unit-testing/SKILL.md`: xUnit, FluentAssertions, real domain objects,
mock only `I*Repository`. Helpers: `HouseTestDataBuilder`, `PlayerTestDataBuilder`,
`MapTestDataBuilder`, `ItemTestDataBuilder`.

### `SetNewOwner` / `HouseService`

- Owner A → owner B with `updatePaidUntil: false` → `PaidUntil` unchanged, `PayRentWarnings == 0`,
  access lists cleared, `SetOwner` still evicts / wakes / depots (existing tests cover seams).

### `House` pending transfer

- `TryAttachTransfer` succeeds once; second attach fails until `ResetTransfer` / `ExecuteTransfer`.
- `ExecuteTransfer` with a different item returns false and does not clear a live pending item.

### `HouseTradeService` (Category `Validation` / `HappyPath`)

Use two players on a small map (same pattern as `SafeTrade` tests).

| Case | Expected |
|---|---|
| Partner > 2 sqm or different floor | `TradePlayerFarAway`, no trade window, no pending item |
| Seller is not owner (guest / subowner / stranger) | `YouDontOwnThisHouse` |
| Partner already owns any house | `TradePlayerAlreadyOwnsAHouse` |
| Happy path start | `NoError`, `PendingTransfer` set, both players in `TradeRequestTracker` |
| Start while pending | `YouCannotTradeThisHouse` |
| Seller already in another trade | `NoError`, pending cleared, existing SafeTrade cancel message |

### SafeTrade + house document (`TradeCompletionTests` / new `HouseTradeTests`)

| Case | Expected |
|---|---|
| Both accept (seller document + buyer item) | Buyer is owner; seller’s item is the buyer’s offer; document not in either inventory; `PendingTransfer` null |
| Cancel / walk away | Owner unchanged; `PendingTransfer` null |
| Look text | Contains `house transfer document for '{Name}'` |

Do not mock `HouseService` or `SafeTradeSystem`. Mock `IHouseRepository` only.

---

## In-game smoke checklist

- [ ] Owner inside house, partner online within 2 sqm same floor: `!sellhouse Name` opens trade
      showing a document on both clients; partner also sees “{Owner} wants to trade with you.”
- [ ] Partner offers gold, both accept → partner owns the house; gold is in the seller’s inventory;
      document is gone; guest/subowner/door lists empty; remaining rent (`PaidUntil`) unchanged
- [ ] Pickupables from the house appear in the **old** owner depot (config flag on)
- [ ] Occupants who are not the new owner are at the house exit
- [ ] `!sellhouse` with no name / unknown / self → “Trade player not found.”
- [ ] Cast from street / neighbor house → “You must stand in your house to initiate the trade.”
- [ ] Subowner inside the house → “You don't own this house.”
- [ ] Partner already owns a house → “Trade player already owns a house.”
- [ ] Partner 3 sqm away or different floor → “Trade player is too far away.”
- [ ] Second `!sellhouse` while the first window is open → “You can not trade this house.”
- [ ] Cancel or walk > 2 sqm from partner → trade closes, owner unchanged, a later `!sellhouse` works
- [ ] Command text is not shown in public chat

---

## Out of scope

- `!leavehouse`
- Auctions / `TRADEPLAYERHIGHESTBIDDER` (always allow)
- Premium check on the buyer
- CipSoft scheduled website transfer (UC-08..UC-10)
- Pricing the house server-side (payment is whatever the buyer puts in the trade)
- Full `HouseFunctions` surface (tiles, beds, rent getters, `save`)

---

## Suggested file list

| File | Change |
|---|---|
| `src/Core/NeoServer.Domain/Houses/House.cs` | Pending transfer + warning reset on owner change |
| `src/Core/NeoServer.Domain/Houses/IHouseTransferItem.cs` | New |
| `src/Core/NeoServer.Domain/Houses/HouseTransferItem.cs` | New |
| `src/Core/NeoServer.Domain/Houses/HouseTradeResult.cs` | New |
| `src/Core/NeoServer.Domain/Houses/Services/IHouseTradeService.cs` | New |
| `src/Core/NeoServer.Domain/Houses/Services/HouseTradeService.cs` | New |
| `src/Core/NeoServer.Domain/Common/Contracts/Services/ITradeService.cs` | Add `Request` |
| `src/Core/NeoServer.Domain/SafeTrade/Validations/TradeRequestValidation.cs` | Skip sight / next-to for transfer item |
| `src/Core/NeoServer.Domain/SafeTrade/Operations/TradeItemExchanger.cs` | Token exchange + `Complete` |
| `src/Core/NeoServer.Domain/SafeTrade/SafeTradeSystem.cs` | `Cancel` → `IHouseTransferItem.Cancel` |
| `src/Standalone/IoC/Modules/ServiceInjection.cs` | Register `IHouseTradeService` |
| `src/Extensions/NeoServer.Scripts.LuaJIT/Functions/HouseFunctions.cs` | `startTrade` |
| `data/scripts/talkactions/house/sellhouse.lua` | New |
| `tests/NeoServer.Domain.Tests/Houses/` | New trade tests + `SetNewOwner` warning case |
| `docs/house-system/house-phase3-steps.md` | Slice 5 tracker |
