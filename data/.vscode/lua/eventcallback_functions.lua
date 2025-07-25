---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class EventCallback
EventCallback = {}

---Sets the type of the event callback.
---@param typeName string
---@return boolean
function EventCallback:type(typeName) end

---Registers the event callback.
---@return boolean
function EventCallback:register() end

_G.EventCallback = EventCallback  -- Set as global