---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Action
Action = {}

---Registers a callback for action use.
---@param callback fun(action: Action)
function Action:onUse(callback) end

---Registers the action in the system.
function Action:register() end

---Sets item ids for the action.
---@param ids number|number[]
function Action:id(ids) end

---Sets action ids for the action.
---@param ids number|number[]
function Action:aid(ids) end

---Sets unique ids for the action.
---@param ids number|number[]
function Action:uid(ids) end

---Allows using the action from a distance.
---@param allow boolean
function Action:allowFarUse(allow) end

---Blocks using the action through walls.
---@param block boolean
function Action:blockWalls(block) end

---Checks the floor when using the action.
---@param check boolean
function Action:checkFloor(check) end

_G.Action = Action  -- Set as global for LSP

return Action