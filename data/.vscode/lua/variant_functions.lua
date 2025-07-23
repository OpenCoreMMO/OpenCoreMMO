---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Variant
Variant = {}

---Gets the number value from the variant.
---@return number
function Variant:getNumber() end

---Gets the string value from the variant.
---@return string
function Variant:getString() end

---Gets the position value from the variant.
---@return table
function Variant:getPosition() end

_G.Variant = Variant  -- Set as global for LSP

return Variant