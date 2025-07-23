---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Logger
Logger = {}

---Logs an info message.
---@param value string
---@param message? string
function Logger:info(value, message) end

---Logs a warning message.
---@param value string
---@param message? string
function Logger:warn(value, message) end

---Logs an error message.
---@param value string
---@param message? string
function Logger:error(value, message) end

---Logs a debug message.
---@param value string
---@param message? string|nil
function Logger:debug(value, message) end

_G.logger = Logger  -- Set as global for LSP

return Logger