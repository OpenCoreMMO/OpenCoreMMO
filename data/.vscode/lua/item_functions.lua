---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Item
Item = {}

---Checks if the object is an item.
---@return boolean
function Item:isItem() end

---Checks if the item is a container.
---@return boolean
function Item:isContainer() end

---Gets the item id.
---@return number
function Item:getId() end

---Removes the item or reduces its count.
---@param count number|nil
---@return boolean
function Item:remove(count) end

---Gets the unique id of the item.
---@return number
function Item:getUniqueId() end

---Gets the action id of the item.
---@return number
function Item:getActionId() end

---Sets the action id of the item.
---@param id number
---@return boolean
function Item:setActionId(id) end

---Gets the subtype of the item.
---@return number
function Item:getSubType() end

---Gets the fluid type of the item.
---@return number
function Item:getFluidType() end

---Gets the name of the item.
---@return string
function Item:getName() end

---Gets the plural name of the item.
---@return string
function Item:getPluralName() end

---Gets the article of the item.
---@return string
function Item:getArticle() end

---Gets the position of the item.
---@return table
function Item:getPosition() end

---Gets the tile where the item is located.
---@return Tile|nil
function Item:getTile() end

---Checks if the item has a property.
---@param property number
---@return boolean
function Item:hasProperty(property) end

---Checks if the item has an attribute.
---@param key number|string
---@return boolean
function Item:hasAttribute(key) end

---Gets an attribute value from the item.
---@param key number|string
---@return any
function Item:getAttribute(key) end

---Sets an attribute value for the item.
---@param key number|string
---@param value any
---@return boolean
function Item:setAttribute(key, value) end

---Removes an attribute from the item.
---@param key number|string
---@return boolean
function Item:removeAttribute(key) end

---Checks if the item has a custom attribute.
---@param key number|string
---@return boolean
function Item:hasCustomAttribute(key) end

---Gets a custom attribute value from the item.
---@param key number|string
---@return any
function Item:getCustomAttribute(key) end

---Sets a custom attribute value for the item.
---@param key number|string
---@param value any
---@return boolean
function Item:setCustomAttribute(key, value) end

---Removes a custom attribute from the item.
---@param key number|string
---@return boolean
function Item:removeCustomAttribute(key) end

---Moves the item to a position or cylinder.
---@param target any
---@param flags number|nil
---@return boolean
function Item:moveTo(target, flags) end

---Transforms the item into another item.
---@param itemId number|string
---@param subType number|nil
---@return boolean
function Item:transform(itemId, subType) end

---Starts the decay process for the item.
---@param decayId number|nil
---@return boolean
function Item:decay(decayId) end

_G.Item = Item  -- Set as global for LSP

return Item