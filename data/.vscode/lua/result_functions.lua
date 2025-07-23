---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Result
Result = {}

---Gets a number value from the result.
---@param id number
---@param column string
---@return number|boolean
function Result:getNumber(id, column) end

---Gets a string value from the result.
---@param id number
---@param column string
---@return string|boolean
function Result:getString(id, column) end

---Moves to the next row in the result.
---@param id number
---@return boolean
function Result:next(id) end

---Frees the result from memory.
---@param id number
---@return boolean
function Result:free(id) end

_G.Result = Result  -- Set as global for LSP

return Result