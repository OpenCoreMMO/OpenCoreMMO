# House System - Domain

## Description

In Tibia, players with a Premium Account have the option to rent a house or shop for storing items and sleeping. Leaders of a Guild also have the ability to rent a Guildhall for everyone in their guild to call home.

## Benefits

Having a house offers many great advantages, here are a few of them:

- Controlling who has access to the house, and who can open/close each door.
- Storing an infinite number of items.
- Being able to sleep in a bed to regenerate Hitpoints, Mana, Soul Points and train your Skills while offline.
- Safely display your items to other players.
- Protection Zone.

## Auctions Use Cases

### UC-01: Place a Bid (Updated)

**Actor:** Authenticated Premium Account Player

**Precondition:** Player has a Premium Account and sufficient bank balance.

**Main Flow:**
1. Player selects a property and enters a bid amount.
2. System validates the player holds a Premium Account.
3. System validates the bid does not exceed the player's current bank balance.
4. System reserves the bid amount in the player's bank account.
5. System registers the bid.

**Alternate Flows:**
- Player does not have a Premium Account → system rejects the bid.
- Bid exceeds bank balance → system rejects the bid.

### UC-02: Modify a Bid

**Actor:** Authenticated Player with an active bid

**Precondition:** Auction is still ongoing; player has already placed a bid.

**Main Flow:**
1. Player chooses to raise or lower their current bid.
2. System validates the new amount against available balance (for raises).
3. System updates the reserved amount accordingly.

**Business Rule:** Player cannot remove a bid entirely — only modify it.

### UC-03: Be Overbid

**Actor:** System (triggered when another player places a higher bid)

**Main Flow:**
1. A second player places a higher bid.
2. System releases the reservation on the previous highest bidder's bank account.
3. Funds become available again for the overbid player.

### UC-04: Win Auction as Premium Account Player

**Actor:** Premium Account Player

**Precondition:** Auction ends with player as highest bidder.

**Main Flow:**
1. System identifies the winning bidder.
2. System deducts the winning bid amount plus first rent from the player's bank account.
3. System assigns the house to the player.
4. System schedules automatic rent deductions every 30 days.

### UC-05: Win Auction as Free Account Player

**Actor:** Free Account Player

**Precondition:** Auction ends with player as highest bidder.

**Main Flow:**
1. System identifies the winning bidder.
2. System deducts the winning bid plus first rent from the player's bank account.
3. System does not assign the house — puts it back on auction.

**Note:** This is a cautionary case players should consider before bidding.

### UC-06: Premium Account Expires During Auction

**Actor:** System / Player

**Main Flow:**
1. Player's Premium Account expires while they have an active bid.
2. Auction continues normally without interruption.
3. Outcome is determined at auction end based on account type at the time of winning (UC-04 or UC-05).

### UC-07: Automatic Rent Deduction

**Actor:** System (scheduled)

**Precondition:** Player owns a house obtained via auction.

**Main Flow:**
1. Every 30 days, system debits the rent fee from the player's bank account.
2. If balance is insufficient, system applies relevant penalty logic (implied).

## Transfer

### UC-08: Initiate House Transfer (Seller)

**Actor:** Premium Account Player (House Owner / Seller)

**Precondition:** Player owns a house and wants to transfer it to another player.

**Main Flow:**
1. Seller logs in to the Tibia home page or opens the Cyclopedia.
2. Seller navigates to Community → Houses, selects server and city, and locates their house.
3. Seller clicks "Move Out" and checks "Yes, I want to leave my house."
4. Seller sets the transfer date, the receiver character name, and the transfer price.
5. Seller submits the transfer request.
6. System registers the pending transfer and awaits buyer acceptance.

**Alternate Flow:**
- Seller does not fill all required fields → system prevents submission.

### UC-09: Accept House Transfer (Buyer)

**Actor:** Any Player (Buyer)

**Precondition:** A pending transfer exists addressed to the buyer's character.

**Main Flow:**
1. Buyer logs in to the Tibia home page or checks the house on the Cyclopedia.
2. Buyer reviews the transfer details (house, date, and price).
3. Buyer accepts the transfer.
4. System schedules the transfer to execute after server save on the specified date.

**Alternate Flow:**
- Buyer does not accept → transfer does not proceed; house remains with the seller.

### UC-10: Execute House Transfer

**Actor:** System (triggered after server save on transfer date)

**Precondition:** Buyer has accepted the transfer.

**Main Flow:**
1. System deducts the transfer price from the buyer's bank account and credits the seller.
2. System assigns house ownership to the buyer, including all items left inside by the seller.
3. System inherits the existing rent due date from the previous owner and debits rent from the buyer's bank account on that same date.
4. System schedules subsequent rent deductions every 30 days from that date.

**Alternate Flow:**
- Transfer occurs exactly on the rent due date → buyer pays the rent on that same day the transfer takes place.

**Business Rule:** The seller is responsible for emptying the house before the transfer date. If they fail to do so, all remaining items become the buyer's property — the system performs no validation or blocking on house contents.

## Losing the House

### UC-11: Lose House Due to Unpaid Rent

**Actor:** System (scheduled check)

**Precondition:** Rent due date has passed and player has not paid within the 7-day grace period.

**Main Flow:**
1. System detects the rent has not been paid within 7 days after the due date.
2. System evicts the player, removing house ownership.
3. System applies a 30-day auction and direct purchase ban to all characters on the player's account.
4. After 30 days, system lifts the ban and allows normal bidding and buying again.

**Business Rule:** During the 30-day penalty period, the player cannot bid on auctions nor buy houses directly from other players.

### UC-12: Lose House Due to Premium Account Expiry

**Actor:** System (triggered at server save)

**Precondition:** Player's Premium Account has expired and they still own a house.

**Main Flow:**
1. Player's Premium Account expires.
2. At the next server save, system removes house ownership from the player.
3. System moves all items left inside the house to the player's depot.

**Alternate Flow — Transfer Saves the House:**
1. Player's Premium Account expires but a house transfer to another player was already accepted and scheduled for the same server save.
2. System processes the transfer before applying the house loss.
3. Buyer receives the house normally (with any items inside, per UC-10 rules); seller avoids eviction.

**Business Rules:**
- A Free Account player can still initiate and complete a house transfer; Premium is not required for the transfer itself.

### UC-13: Pay Rent While Banished

**Actor:** Banished Player / Friend of Banished Player

**Precondition:** Player is banished and cannot log in to deposit funds for rent payment.

**Main Flow:**
1. Player is banished and risks not having enough money in their bank account for the upcoming rent.
2. A friend transfers money directly to the banished player's bank account.
3. System debits the rent from the player's bank account on the due date as normal, regardless of banishment status.

**Business Rule:** Banishment does not suspend or delay rent collection — the player is still liable for rent on schedule.

## Managing the House

### UC-14: Manage House Access (Owner)

**Actor:** House Owner

**Precondition:** Player owns a house.

**Main Flow:**
1. Owner uses in-game house spells to manage access settings.
2. Owner grants or revokes invitations to specific characters.
3. Owner assigns door-level permissions to specific characters as needed.

**Business Rule:** The house owner always has full and ultimate access to all areas and doors of the house, with no restrictions.

### UC-15: Enter House as Invited Character

**Actor:** Invited Character

**Precondition:** Character has been added to the house invitation list.

**Main Flow:**
1. Invited character approaches the house and enters any area they can physically reach.
2. If a door is open, any invited character may pass through it.
3. If a door is closed, only characters with explicit permission for that door can open or close it.

### UC-16: Manage Door Permissions

**Actor:** House Owner

**Precondition:** Player owns a house with one or more doors.

**Main Flow:**
1. Owner assigns specific characters the right to open/close individual doors using house spells.
2. System restricts closed door interaction to only those characters with that door's permission.

### Guest List

#### UC-17: Manage Guest List

**Actor:** House Owner or Sub-Owner

**Precondition:** Actor is standing inside the house.

**Main Flow:**
1. Owner or Sub-Owner casts the aleta sio spell while inside the house.
2. System opens the guest list management interface.
3. Actor adds or removes characters from the guest list.

**Business Rule:** Only Owners and Sub-Owners can edit the guest list.

#### UC-18: Enter House as Guest

**Actor:** Guest (character on the guest list)

**Precondition:** Character is on the house guest list.

**Main Flow:**
1. Guest approaches the house entrance.
2. If the front door is open, guest enters and may move freely through any room in the house.
3. Guest cannot open or close any doors.

**Alternate Flow:**
- Front door is closed → guest cannot enter, even if they are on the guest list.

**Business Rules:**
- Guests have unrestricted movement through all rooms, but zero door interaction rights.
- A closed front door acts as a hard barrier regardless of guest list membership — door permission is a separate, higher-level access right (UC-21).

### Sub-Owners

#### UC-19: Manage Sub-Owner List

**Actor:** House Owner

**Precondition:** Owner is standing inside the house.

**Main Flow:**
1. Owner casts the aleta som spell while inside the house.
2. System opens the sub-owner list management interface.
3. Owner adds or removes characters from the sub-owner list.
4. System validates that the list does not exceed 10 sub-owners.

**Alternate Flow:**
- Owner attempts to add an 11th sub-owner → system rejects the addition.

**Business Rule:** Only the house Owner (not Sub-Owners) can edit the sub-owner list.

#### UC-20: Act as Sub-Owner

**Actor:** Sub-Owner (Premium Account Player)

**Precondition:** Character is on the sub-owner list and holds an active Premium Account.

**Main Flow:**
1. Sub-Owner enters the house.
2. Sub-Owner may edit the guest list (UC-17).
3. Sub-Owner may kick characters whose house access is not higher than their own (guests, other sub-owners, uninvited occupants). They cannot kick the owner or a player with `CanEditHouses`.

**Alternate Flow:**
- Sub-Owner's Premium Account expires → character remains on the sub-owner list but loses all sub-owner abilities until Premium is restored.

**Business Rules:**
- Sub-owner abilities are only active for Premium Account players.
- A Free Account character on the sub-owner list is inactive — they hold the slot but cannot exercise any sub-owner rights.
- Maximum of 10 sub-owners per house.

### Door Access

#### UC-21: Manage Door Access List (Owner)

**Actor:** House Owner

**Precondition:** Owner is facing the specific door they want to manage.

**Main Flow:**
1. Owner faces the target door and casts the aleta grav spell.
2. System opens the door access list management interface for that specific door.
3. Owner adds or removes characters from that door's access list.

**Business Rule:** Each door in the house — including the front door — has its own independent access list. Changes to one door's list do not affect any other door.

#### UC-22: Interact with a Door as Authorized Character

**Actor:** Character with door access permission

**Precondition:** Character is on the access list for the specific door.

**Main Flow:**
1. Character approaches the door.
2. Character opens or closes the door.

**Alternate Flow:**
- Character is not on that door's access list → character cannot open or close the door, even if they are a guest or on another door's access list.

**Business Rules:**
- Door access lists follow the same mechanics as the Guest List (UC-17 and UC-18).
- Being on the guest list alone does not grant door interaction rights — door access must be explicitly assigned per door.
- The Owner always retains full access to all doors regardless of any list configuration.

### House Spells

#### UC-23: Cast aleta sio — Edit Guest List

**Actor:** House Owner or Sub-Owner

**Precondition:** Actor is standing inside the house.

**Main Flow:**
1. Actor casts aleta sio while inside the house.
2. System opens the guest list interface.
3. Actor adds or removes characters from the guest list.

(See UC-17 for full guest list behavior.)

#### UC-24: Cast aleta som — Edit Sub-Owner List

**Actor:** House Owner

**Precondition:** Owner is standing inside the house.

**Main Flow:**
1. Owner casts aleta som while inside the house.
2. System opens the sub-owner list interface.
3. Owner adds or removes characters from the sub-owner list.

**Business Rule:** Only the main Owner can cast this spell — Sub-Owners cannot edit the sub-owner list.

(See UC-19 for full sub-owner list behavior.)

#### UC-25: Cast alana sio "character" — Kick Character

**Actor:** Any player whose house access is not lower than the target's

**Precondition:** Target character is standing on a tile of the house.

**Main Flow:**
1. Actor casts alana sio "character", replacing character with the target's name. With no name, the caster is the target.
2. System resolves the house from the **target's** tile (not the caster's).
3. If the caster's house access is not lower than the target's, and the target does not have `CanEditHouses`, the system teleports the target to the house front door (entry).
4. Otherwise the cast fails (cancel + POFF). The target stays put.

**Alternate Flow — Self-Kick:**
1. A character inside a house casts the spell with no name, or targeting themselves.
2. Access levels are equal, so the kick succeeds unless they have `CanEditHouses`.
3. System moves them to the front door.

**Business Rules:**
- Kick is allowed when `GetAccessLevel(caster) >= GetAccessLevel(target)` and the target does not have `CanEditHouses`.
- Equal access can kick (guest kicks guest; self-kick). Lower access cannot (guest cannot kick sub-owner or owner; sub-owner cannot kick owner).
- Players with `CanEditHouses` cannot be kicked. They count as house owner access, so they can kick the recorded owner.
- The spell has unlimited range and can be cast from outside the house — anywhere in the game world — as long as the target is inside a house the caster has sufficient access to.

#### UC-26: Cast aleta grav — Edit Door Access List

**Actor:** House Owner

**Precondition:** Owner is standing in a doorway or facing the door from either side.

**Main Flow:**
1. Owner casts aleta grav while positioned at the target door.
2. System opens the door access list interface for that specific door.
3. Owner adds or removes characters from that door's access list.

**Business Rules:**
- Only the main Owner can cast this spell — Sub-Owners cannot edit door access lists.
- When a door is open, all characters on the house invite list may pass through, even without explicit door rights.
- When a door is closed, only characters on that door's access list can open or close it.

(See UC-21 and UC-22 for full door access behavior.)

### List Syntax

#### UC-27: Add Entry to Invite List Using Exact Name

**Actor:** House Owner or Sub-Owner (depending on the list being edited)

**Precondition:** Actor has permission to edit the target list.

**Main Flow:**
1. Actor types a character's exact name (e.g. Relambelia Roarsia) as a new line entry.
2. System matches and invites only that specific character.

**Business Rule:** List supports at most 100 lines and 1999 characters total. Exceeding either limit reverts to the previous state without saving.

#### UC-28: Use Wildcard to Invite Multiple Characters

**Actor:** House Owner or Sub-Owner

**Precondition:** Actor is editing a guest, door, or sub-owner list.

**Main Flow:**
1. Actor uses wildcard syntax to match multiple character names:
   - `String*` — matches any name starting with the given string, of any length (e.g. `Santor*` matches Santor, Santora, Santorius).
   - `String?` — matches names where ? represents exactly one character (e.g. `Hannibrag?` matches Hannibraga, Hannibrags but not Hannibrag or Hannibragius).
   - `*` alone — invites every existing player in the game.
2. System evaluates the list top-to-bottom and grants access to all matched characters not explicitly excluded.

**Business Rule:** Wildcards carry inherent risk — any character whose name matches the pattern gains access unless explicitly excluded via `!String`.

#### UC-29: Invite Entire Guild or Guild Rank

**Actor:** House Owner or Sub-Owner

**Precondition:** Actor is editing a guest, door, or sub-owner list.

**Main Flow:**
1. Actor uses guild syntax:
   - `*@GuildName` — invites all members of the specified guild.
   - `Rank@GuildName` — invites only members of the specified rank within the guild.
2. System grants access to all matched guild members.

#### UC-30: Exclude a Character from a Wildcard or Guild Match

**Actor:** House Owner or Sub-Owner

**Precondition:** A wildcard or guild entry is already present in the list that would otherwise match the character to be excluded.

**Main Flow:**
1. Actor adds `!CharacterName` as a line entry above any wildcard or guild entries that would match that character.
2. System reads the list top-to-bottom and marks the character as strictly excluded before processing the wildcard.
3. The excluded character is denied access even if a later wildcard or guild entry would match their name.

**Business Rules:**
- Exclusion lines (`!`) must be placed at the top of the list, before any wildcards or guild entries, to function correctly.
- The system does not retroactively apply an exclusion line if the character was already matched by a prior entry — order is strictly top-to-bottom.
- It is best practice to maintain a universal exclusion block at the very top of the list.

#### UC-31: Use Comment or Annotation in List

**Actor:** House Owner or Sub-Owner

**Precondition:** Actor wants to annotate the invite list for readability.

**Main Flow:**
1. Actor uses a tilde (`~`) at the start of a line as a workaround comment — no character name may contain a tilde, so it is safe to use.
2. System ignores the tilde line for access purposes but counts it toward the 100-line limit.

**Alternate Flow — Hash comment (`#`):**
1. Actor types a `#String` line (e.g. `#My best friend`).
2. System ignores the line for access, but removes it upon submission — custom hash comments are not persisted.

**Business Rule:** A system-generated `#` comment exists at the top of the list by default; custom `#` comments are silently removed on submit.
