---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Player : Creature
Player = {}

---Checks if the object is a player.
---@return boolean
function Player:isPlayer() end

---Teleports the player to a position.
---@param position Position
---@param pushMovement? boolean
function Player:teleportTo(position, pushMovement) end

---Gets the player's free capacity.
---@return number
function Player:getFreeCapacity() end

---Gets the player's raw skill level.
---@param skillType number
---@return number
function Player:getSkillLevel(skillType) end

---Gets the player's effective skill level.
---@param skillType number
---@return number
function Player:getEffectiveSkillLevel(skillType) end

---Gets the player's skill percent.
---@param skillType number
---@return number
function Player:getSkillPercent(skillType) end

---Gets the player's skill tries.
---@param skillType number
---@return number
function Player:getSkillTries(skillType) end

---Adds skill tries to the player.
---@param skillType number
---@param tries number
function Player:addSkillTries(skillType, tries) end

---Gets the player's gender.
---@return number
function Player:getSex() end

---Sets the player's gender.
---@param newSex number
function Player:setSex(newSex) end

---Gets the player's mana.
---@return number
function Player:getMana() end

---Adds or removes mana from the player.
---@param manaChange number
---@param animationOnLoss? boolean
function Player:addMana(manaChange, animationOnLoss) end

---Gets the player's spent mana.
---@return number
function Player:getManaSpent() end

---Adds spent mana to the player.
---@param amount number
function Player:addManaSpent(amount) end

---Gets the player's group.
---@return Group
function Player:getGroup() end

---Sets the player's group.
---@param group Group
function Player:setGroup(group) end

---Gets a storage value for the player.
---@param key number
---@return number
function Player:getStorageValue(key) end

---Sets a storage value for the player.
---@param key number
---@param value number
function Player:setStorageValue(key, value) end

---Adds an item to the player.
---@param itemId number
---@param count? number
---@param canDropOnMap? boolean
---@param subType? number
---@param slot? number
---@return Item
function Player:addItem(itemId, count, canDropOnMap, subType, slot) end

---Removes an item from the player.
---@param itemId number
---@param count number
---@param subType? number
---@param ignoreEquipped? boolean
---@return boolean
function Player:removeItem(itemId, count, subType, ignoreEquipped) end

---Sends a text message to the player.
---@param type number
---@param text string
function Player:sendTextMessage(type, text) end

---Checks if the player is protection zone locked.
---@return boolean
function Player:isPzLocked() end

---Sets the player's ghost mode (invisible).
---@param enabled boolean
function Player:setGhostMode(enabled) end

---Feeds the player.
---@param food number
function Player:feed(food) end

---Gets the player's level.
---@return number
function Player:getLevel() end

---Gets the item in the specified slot for the player.
---@param slot number
---@return Item
function Player:getSlotItem(slot) end

_G.Player = Player

return Player