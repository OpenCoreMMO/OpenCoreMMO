---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class TalkAction
TalkAction = {}

---Registers a callback for when the talk action is used.
---@param callback fun(action: TalkAction)
function TalkAction:onSay(callback) end

---Registers the talk action in the system.
---@return boolean
function TalkAction:register() end

---Sets the separator for the talk action.
---@param sep string
---@return boolean
function TalkAction:separator(sep) end

---Gets the words for the talk action.
---@return string
function TalkAction:getName() end

_G.TalkAction = TalkAction  -- Set as global for LSP

return TalkAction