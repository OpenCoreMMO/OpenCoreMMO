---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Town
Town = {}

---Gets the id of the town.
---@return number
function Town:getId() end

---Gets the name of the town.
---@return string
function Town:getName() end

---Gets the temple position of the town.
---@return Position
function Town:getTemplePosition() end

_G.Town = Town  -- Set as global for LSP

return Town