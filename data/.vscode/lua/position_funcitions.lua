---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Position
---@field x number
---@field y number
---@field z number
---@field stackpos number
Position = {}

---Adds two positions.
---@param position Position
---@return Position
function Position:__add(position) end

---Subtracts one position from another.
---@param position Position
---@return Position
function Position:__sub(position) end

---Checks if two positions are equal.
---@param position Position
---@return boolean
function Position:__eq(position) end

---Gets the distance to another position.
---@param position Position
---@return number
function Position:getDistance(position) end

---Gets the path to another position.
---@param position Position
---@param minTargetDist number|nil
---@param maxTargetDist number|nil
---@param fullPathSearch boolean|nil
---@param clearSight boolean|nil
---@param maxSearchDist number|nil
---@return table|boolean
function Position:getPathTo(position, minTargetDist, maxTargetDist, fullPathSearch, clearSight, maxSearchDist) end

---Checks if the sight is clear to another position.
---@param position Position
---@param sameFloor boolean|nil
---@return boolean
function Position:isSightClear(position, sameFloor) end

---Sends a magic effect to the position.
---@param magicEffect number
---@param player Player|nil
---@return boolean
function Position:sendMagicEffect(magicEffect, player) end

---Converts the position to a string.
---@return string
function Position:toString() end

---Gets the next position in a given direction.
---@param direction number
---@return Position
function Position:getNextPosition(direction) end

_G.Position = Position  -- Set as global for LSP

return Position