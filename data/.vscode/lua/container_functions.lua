---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Container : Item
Container = {}

---Gets the number of items in the container.
---@return number
function Container:getSize() end

---Gets the capacity of the container.
---@return number
function Container:getCapacity() end

---Gets the number of empty slots in the container.
---@param recursive boolean|nil
---@return number
function Container:getEmptySlots(recursive) end

---Gets a description of the container's contents.
---@return string
function Container:getContentDescription() end

---Gets all items in the container.
---@param recursive boolean|nil
---@return table
function Container:getItems(recursive) end

---Gets the number of items held by the container.
---@return number
function Container:getItemHoldingCount() end

---Gets the count of items by item id.
---@param itemId number|string
---@param subType number|nil
---@return number
function Container:getItemCountById(itemId, subType) end

---Gets an item by index.
---@param index number
---@return Item|nil
function Container:getItem(index) end

---Checks if the container has a specific item.
---@param item Item
---@return boolean
function Container:hasItem(item) end

---Adds an item to the container by item id.
---@param itemId number|string
---@param count number|nil
---@param index number|nil
---@return Item|boolean
function Container:addItem(itemId, count, index) end

---Adds an item instance to the container.
---@param item Item
---@param index number|nil
---@return boolean
function Container:addItemEx(item, index) end

---Gets the corpse owner id from the container.
---@return number|nil
function Container:getCorpseOwner() end

_G.Container = Container  -- Set as global for LSP

return Container