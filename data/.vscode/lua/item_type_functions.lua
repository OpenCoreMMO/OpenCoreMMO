---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class ItemType
ItemType = {}

---Checks if the item type is a corpse.
---@return boolean
function ItemType:isCorpse() end

---Checks if the item type is movable.
---@return boolean
function ItemType:isMovable() end

---Checks if the item type is stackable.
---@return boolean
function ItemType:isStackable() end

---Checks if the item type is a fluid container.
---@return boolean
function ItemType:isFluidContainer() end

---Checks if the item type is a key.
---@return boolean
function ItemType:isKey() end

---Gets the type id of the item type.
---@return number
function ItemType:getType() end

---Gets the id of the item type.
---@return number
function ItemType:getId() end

---Gets the name of the item type.
---@return string
function ItemType:getName() end

---Gets the weight of the item type.
---@param count number|nil
---@return number
function ItemType:getWeight(count) end

---Gets the destroy id of the item type.
---@return number
function ItemType:getDestroyId() end

_G.ItemType = ItemType  -- Set as global for LSP

return ItemType