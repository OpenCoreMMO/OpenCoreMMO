---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class CreatureEvent
CreatureEvent = {}

---Sets the type of the creature event.
---@param type string
function CreatureEvent:type(type) end

---Registers the creature event in the system.
---@return boolean
function CreatureEvent:register() end

---Registers a callback for the onLogin event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onLogin(callback) end

---Registers a callback for the onLogout event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onLogout(callback) end

---Registers a callback for the onThink event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onThink(callback) end

---Registers a callback for the onPrepareDeath event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onPrepareDeath(callback) end

---Registers a callback for the onDeath event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onDeath(callback) end

---Registers a callback for the onKill event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onKill(callback) end

---Registers a callback for the onAdvance event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onAdvance(callback) end

---Registers a callback for the onModalWindow event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onModalWindow(callback) end

---Registers a callback for the onTextEdit event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onTextEdit(callback) end

---Registers a callback for the onHealthChange event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onHealthChange(callback) end

---Registers a callback for the onManaChange event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onManaChange(callback) end

---Registers a callback for the onExtendedOpcode event.
---@param callback fun(event: CreatureEvent)
function CreatureEvent:onExtendedOpcode(callback) end

_G.CreatureEvent = CreatureEvent  -- Set as global for LSP

return CreatureEvent