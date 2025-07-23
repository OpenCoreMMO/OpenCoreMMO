---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class GlobalEvent
GlobalEvent = {}

---Sets the type of the global event.
---@param type string
function GlobalEvent:type(type) end

---Registers the global event in the system.
---@return boolean
function GlobalEvent:register() end

---Sets the time for the global event.
---@param time string
---@return boolean
function GlobalEvent:time(time) end

---Sets the interval for the global event.
---@param interval number
---@return boolean
function GlobalEvent:interval(interval) end

---Registers a callback for the onThink event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onThink(callback) end

---Registers a callback for the onTime event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onTime(callback) end

---Registers a callback for the onStartup event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onStartup(callback) end

---Registers a callback for the onShutdown event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onShutdown(callback) end

---Registers a callback for the onRecord event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onRecord(callback) end

---Registers a callback for the onPeriodChange event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onPeriodChange(callback) end

---Registers a callback for the onSave event.
---@param callback fun(event: GlobalEvent)
function GlobalEvent:onSave(callback) end

_G.GlobalEvent = GlobalEvent  -- Set as global for LSP

return GlobalEvent