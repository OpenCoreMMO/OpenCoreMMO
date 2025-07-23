---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class MoveEvent
MoveEvent = {}

---Sets the type of the move event.
---@param type string
function MoveEvent:type(type) end

---Registers the move event in the system.
---@return boolean
function MoveEvent:register() end

---Sets the required minimum level for the move event.
---@param level number
---@return boolean
function MoveEvent:level(level) end

---Sets the required minimum magic level for the move event.
---@param magicLevel number
---@return boolean
function MoveEvent:magicLevel(magicLevel) end

---Sets the required premium status for the move event.
---@param premium boolean
---@return boolean
function MoveEvent:premium(premium) end

---Sets the required vocation for the move event.
---@param vocation string
---@param showInDescription boolean|nil
---@param lastVoc boolean|nil
---@return boolean
function MoveEvent:vocation(vocation, showInDescription, lastVoc) end

---Sets item ids for the move event.
---@param ids number|number[]
---@return boolean
function MoveEvent:id(ids) end

---Sets a range of item ids for the move event.
---@param fromId number
---@param toId number
---@return boolean
function MoveEvent:idRange(fromId, toId) end

---Sets action ids for the move event.
---@param ids number|number[]
---@return boolean
function MoveEvent:aid(ids) end

---Sets unique ids for the move event.
---@param ids number|number[]
---@return boolean
function MoveEvent:uid(ids) end

---Sets positions for the move event.
---@param positions table|table[]
---@return boolean
function MoveEvent:position(positions) end

---Registers a callback for the onStepIn event.
---@param callback fun(event: MoveEvent)
function MoveEvent:onStepIn(callback) end

---Registers a callback for the onStepOut event.
---@param callback fun(event: MoveEvent)
function MoveEvent:onStepOut(callback) end

---Registers a callback for the onAddItem event.
---@param callback fun(event: MoveEvent)
function MoveEvent:onAddItem(callback) end

---Registers a callback for the onRemoveItem event.
---@param callback fun(event: MoveEvent)
function MoveEvent:onRemoveItem(callback) end

_G.MoveEvent = MoveEvent  -- Set as global for LSP

return MoveEvent