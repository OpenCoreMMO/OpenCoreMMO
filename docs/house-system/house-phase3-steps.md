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

## Later slices (not started)

- [ ] `aleta som` — House Subowner List (`invite_subowners.lua`)
- [ ] `aleta grav` — House Door List (`edit_door.lua` + `getDoorIdByPosition`)
- [ ] `alana sio` — House Kick (`kick_guest.lua`)
- [ ] Talkactions: `buyhouse` / `leavehouse` / `sellhouse`
- [ ] Full `HouseFunctions` surface (tiles, doors, beds, rent, trade, save)
- [ ] Rent warning letters
