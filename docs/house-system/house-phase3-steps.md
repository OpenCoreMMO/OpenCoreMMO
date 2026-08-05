# House System — Phase 3 Step-by-Step (Lua + spells, spell-by-spell)

> Companion to `house-phase1-steps.md`, `house-phase2-steps.md`, and the master plan
> `House-System-Plan.md` ("Phase 3 — Lua handlers + scripts").
>
> Phase 3 is delivered **spell-by-spell**. This document tracks each slice.

---

## Slice 1 — `aleta sio` (House Guest List) — DONE

Reference behavior: guest-list spell + house window packets `0x97` / `0x8A`.

### What shipped

1. **House window protocol**
   - Outgoing `HouseWindow = 0x97` → `HouseWindowPacket`
   - Incoming `WindowHouse = 0x8A` → `UpdateHouseWindowPacket` + `PlayerHouseWindowHandler`
   - Edit session store: `HouseEditWindowStore` (windowTextId, houseId, listId per player)

2. **Save path**
   - Handler → `PlayerEditHouseAccessListCommand`
   - After guest/subowner list save: kick uninvited occupants via `IHouseEviction`

3. **Minimal Lua**
   - `Creature:getTile()`, `Tile:getHouse()`
   - Slim `HouseFunctions`: `House(id)`, `getId`, `canEditAccessList`, `getAccessList`
   - Globals `GUEST_LIST` / `SUBOWNER_LIST`
   - `Player:setEditHouse`, `Player:sendHouseWindow`

4. **Spell**
   - `data/scripts/spells/house/invite_guests.lua` — words `aleta sio`

### In-game smoke checklist

- [ ] Owner on house tile casts `aleta sio` → guest list window opens
- [ ] Subowner on house tile casts `aleta sio` → window opens
- [ ] Guest / stranger cast → cancel + POFF
- [ ] Outside house cast → cancel + POFF
- [ ] Save list with a name → persists; removed guest on a house tile is teleported to exit

---

## Slice 2 — `aleta som` (House Subowner List) — DONE

### What shipped

- `data/scripts/spells/house/invite_subowners.lua` — words `aleta som`, spell id **251**, `SUBOWNER_LIST` access list
- Owner-only behavior enforced by existing domain rule `House.CanEditAccessList` (owner can edit subowner list; subowner cannot)
- Save path already kicks uninvited occupants after subowner list updates via `IHouseEviction` (`HouseEvictionService.KickUninvited`)
- **Max 10 sub-owners** — `HouseConfiguration.MaxSubOwnerCount` (default 10) enforced in `PlayerEditHouseAccessListCommand`; exclusions/comments don't occupy a slot
- **Premium-gated sub-owner abilities** — `House.RequirePremiumForSubOwners` (wired from `HouseConfiguration.RequirePremiumForSubOwners`, default true): free-account characters keep their slot on the list but lose all sub-owner rights (guest-list editing, kicking, door access, entry) until premium is restored. Independent from `RequirePremiumAccount` (ownership).

### In-game smoke checklist

- [ ] Owner on house tile casts `aleta som` → subowner list window opens
- [ ] Subowner casts `aleta som` → cancel + POFF
- [ ] Guest / stranger / outside house cast → cancel + POFF
- [ ] Owner removes a subowner name and saves → that player (if inside) teleported to exit; list persists
- [ ] Owner saves an 11th subowner → rejected with "may contain at most 10 characters"
- [ ] Free-account character on the subowner list loses entry + subowner abilities; regains them when premium is restored

---

## Later slices (not started)

- [ ] `aleta grav` — House Door List (`edit_door.lua` + `getDoorIdByPosition`)
- [ ] `alana sio` — House Kick (`kick_guest.lua`)
- [ ] Talkactions: `buyhouse` / `leavehouse` / `sellhouse`
- [ ] Full `HouseFunctions` surface (tiles, doors, beds, rent, trade, save)
- [ ] Rent warning letters
