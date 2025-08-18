---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Combat
Combat = {}

---Sets a parameter for the combat object.
---@param key number
---@param value number|boolean
function Combat:setParameter(key, value) end

---Sets the formula for damage calculation.
---@param type number
---@param minA number
---@param minB number
---@param maxA number
---@param maxB number
function Combat:setFormula(type, minA, minB, maxA, maxB) end

---Sets the area of effect for the combat.
---@param area table
---@param extArea table|nil
function Combat:setArea(area, extArea) end

---Adds a condition to the combat.
---@param condition table
function Combat:addCondition(condition) end

---Sets a callback for the combat.
---@param key number
---@param callback fun(...)
function Combat:setCallback(key, callback) end

---Sets the origin for the combat (not implemented).
function Combat:setOrigin() end

---Executes the combat action.
---@param creature table
---@param variant any
function Combat:execute(creature, variant) end

_G.Combat = Combat  -- Set as global for LSP

return Combat