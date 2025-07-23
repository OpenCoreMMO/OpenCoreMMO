---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Creature
Creature = {}

---Gets the events of the creature by type.
---@param type number
---@return string[]
function Creature:getEvents(type) end

---Registers an event for the creature.
---@param name string
---@return boolean
function Creature:registerEvent(name) end

---Unregisters an event for the creature.
---@param name string
---@return boolean
function Creature:unregisterEvent(name) end

---Checks if the object is a creature.
---@return boolean
function Creature:isCreature() end

---Checks if the creature is in ghost mode.
---@return boolean
function Creature:isInGhostMode() end

---Gets the creature's id.
---@return number
function Creature:getId() end

---Gets the creature's name.
---@return string
function Creature:getName() end

---Gets the creature's position.
---@return table
function Creature:getPosition() end

---Gets the creature's direction.
---@return number
function Creature:getDirection() end

---Gets the creature's health.
---@return number
function Creature:getHealth() end

---Sets the creature's health.
---@param health number
---@return boolean
function Creature:setHealth(health) end

---Adds health to the creature.
---@param healthChange number
---@param combatType? number
---@return boolean
function Creature:addHealth(healthChange, combatType) end

---Gets the creature's max health.
---@return number
function Creature:getMaxHealth() end

---Sets the creature's max health.
---@param maxHealth number
---@return boolean
function Creature:setMaxHealth(maxHealth) end

---Sets whether the creature's health is hidden.
---@param hide boolean
---@return boolean
function Creature:setHiddenHealth(hide) end

---Gets a condition from the creature.
---@param conditionType number
---@param conditionId? number
---@param subId? number
---@return Condition|nil
function Creature:getCondition(conditionType, conditionId, subId) end

---Adds a condition to the creature.
---@param condition Condition
---@return boolean
function Creature:addCondition(condition) end

---Removes a condition from the creature.
---@param conditionType number
---@return boolean
function Creature:removeCondition(conditionType) end

---Checks if the creature has a condition.
---@param conditionType number
---@return boolean
function Creature:hasCondition(conditionType) end

---Makes the creature say something.
---@param text string
---@param type number
---@param ghost? boolean
---@param target? Creature
---@param position? table
---@return boolean
function Creature:say(text, type, ghost, target, position) end

---Gets the summons of the creature.
---@return Creature[]
function Creature:getSummons() end

---Moves the creature in a direction or to a tile.
---@param directionOrTile number|Tile
---@param flags? number
---@return number
function Creature:move(directionOrTile, flags) end

---Removes the creature.
---@param forced? boolean
---@return boolean
function Creature:remove(forced) end

_G.Creature = Creature

return Creature