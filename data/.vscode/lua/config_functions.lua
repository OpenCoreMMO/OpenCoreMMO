---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class ConfigManager
ConfigManager = {}

---Gets a string value from the configuration.
---@param key number
---@return string
function ConfigManager:getString(key) end

---Gets a number value from the configuration.
---@param key number
---@return number
function ConfigManager:getNumber(key) end

---Gets a boolean value from the configuration.
---@param key number
---@return boolean
function ConfigManager:getBoolean(key) end

---Gets a float value from the configuration.
---@param key number
---@return number
function ConfigManager:getFloat(key) end

_G.ConfigManager = ConfigManager  -- Set as global for LSP

return ConfigManager