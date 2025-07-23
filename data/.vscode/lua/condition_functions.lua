---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Condition
Condition = {}

---Sets a parameter for the condition object.
---@param key number
---@param value number|boolean
function Condition:setParameter(key, value) end

---Sets the formula for the condition.
---@param minA number
---@param minB number
---@param maxA number
---@param maxB number
function Condition:setFormula(minA, minB, maxA, maxB) end

_G.Condition = Condition  -- Set as global for LSP

return Condition