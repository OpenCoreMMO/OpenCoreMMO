---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Tile
Tile = {}

---Gets the position of the tile.
---@return table
function Tile:getPosition() end

---Gets the ground item of the tile.
---@return Item|nil
function Tile:getGround() end

---Gets a thing (creature or item) by index.
---@param index number
---@return Creature|Item|nil
function Tile:getThing(index) end

---Gets the total number of things on the tile.
---@return number
function Tile:getThingCount() end

---Gets the number of creatures on the tile.
---@return number
function Tile:getCreatureCount() end

---Gets all creatures on the tile.
---@return Creature[]|nil
function Tile:getCreatures() end

---Gets the top visible thing. Optional creature is the observer (TFS: canSeeCreature).
---@param creature? Creature
---@return Creature|Item|nil
function Tile:getTopVisibleThing(creature) end

---Gets the top creature on the tile.
---@return Creature|nil
function Tile:getTopCreature() end

---Gets the top top item on the tile.
---@return Item|nil
function Tile:getTopTopItem() end

---Gets the top down item on the tile.
---@return Item|nil
function Tile:getTopDownItem() end

---Gets all items on the tile.
---@return Item[]
function Tile:getItems() end

---Gets an item on the tile by id.
---@param itemId number
---@param subType? number
---@return Item|nil
function Tile:getItemById(itemId, subType) end

---Gets the total number of items on the tile.
---@return number
function Tile:getItemCount() end

---Gets the number of down items (not always on top) on the tile.
---@return number
function Tile:getDownItemCount() end

---Gets the number of top items (always on top) on the tile.
---@return number
function Tile:getTopItemCount() end

---Checks if the tile has a property. Optionally exclude an item.
---@param property number
---@param item? Item
---@return boolean
function Tile:hasProperty(property, item) end

---Checks if the tile has a flag.
---@param flag number
---@return boolean
function Tile:hasFlag(flag) end

---Queries if a thing can be added to the tile with flags.
---@param thing Creature|Item
---@param flags? number
---@return number
function Tile:queryAdd(thing, flags) end

_G.Tile = Tile

return Tile