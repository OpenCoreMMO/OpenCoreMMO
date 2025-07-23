---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Group
Group = {}

---Gets the group id.
---@return number
function Group:getId() end

---Gets the group name.
---@return string
function Group:getName() end

---Gets the group flags as a number.
---@return number
function Group:getFlags() end

---Checks if the group has access.
---@return boolean
function Group:getAccess() end

---Gets the maximum number of depot items for the group.
---@return number
function Group:getMaxDepotItems() end

---Gets the maximum number of VIP entries for the group.
---@return number
function Group:getMaxVipEntries() end

---Checks if the group has a specific flag.
---@param flag number
---@return boolean
function Group:hasFlag(flag) end

_G.Group = Group  -- Set as global for LSP

return Group