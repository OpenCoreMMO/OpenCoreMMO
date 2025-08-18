---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class NpcType
NpcType = {}

---Gets or sets the name of the NPC type.
---@param name string|nil
---@return string|boolean
function NpcType:name(name) end

---Gets or sets the description of the NPC type.
---@param description string|nil
---@return string|boolean
function NpcType:nameDescription(description) end

---Gets or sets the health of the NPC type.
---@param health number|nil
---@return number|boolean
function NpcType:health(health) end

---Gets or sets the max health of the NPC type.
---@param maxHealth number|nil
---@return number|boolean
function NpcType:maxHealth(maxHealth) end

---Gets the voices of the NPC type.
---@return table
function NpcType:getVoices() end

---Adds voices to the NPC type.
---@param interval number
---@param chance number
---@param ... table
---@return boolean
function NpcType:addVoices(interval, chance, ...) end

---Registers an event for the NPC type.
---@param name string
---@return boolean
function NpcType:registerEvent(name) end

---Sets the event type for the NPC type.
---@param eventType number
---@return boolean
function NpcType:eventType(eventType) end

---Registers a callback for the onThink event.
---@param callback fun(npcType: NpcType)
function NpcType:onThink(callback) end

---Registers a callback for the onAppear event.
---@param callback fun(npcType: NpcType)
function NpcType:onAppear(callback) end

---Registers a callback for the onDisappear event.
---@param callback fun(npcType: NpcType)
function NpcType:onDisappear(callback) end

---Registers a callback for the onMove event.
---@param callback fun(npcType: NpcType)
function NpcType:onMove(callback) end

---Registers a callback for the onSay event.
---@param callback fun(npcType: NpcType)
function NpcType:onSay(callback) end

---Registers a callback for the onCloseChannel event.
---@param callback fun(npcType: NpcType)
function NpcType:onCloseChannel(callback) end

---Registers a callback for the onBuyItem event.
---@param callback fun(npcType: NpcType)
function NpcType:onBuyItem(callback) end

---Registers a callback for the onSellItem event.
---@param callback fun(npcType: NpcType)
function NpcType:onSellItem(callback) end

---Registers a callback for the onCheckItem event.
---@param callback fun(npcType: NpcType)
function NpcType:onCheckItem(callback) end

---Gets or sets the outfit of the NPC type.
---@param outfit table|nil
---@return table|boolean
function NpcType:outfit(outfit) end

---Gets or sets the base speed of the NPC type.
---@param speed number|nil
---@return number|boolean
function NpcType:baseSpeed(speed) end

---Gets or sets the walk interval of the NPC type.
---@param interval number|nil
---@return number|boolean
function NpcType:walkInterval(interval) end

---Gets or sets the walk radius of the NPC type.
---@param radius number|nil
---@return number|boolean
function NpcType:walkRadius(radius) end

---Adds a shop item to the NPC type.
---@param itemId number
---@param buyPrice number
---@param sellPrice number
---@return boolean
function NpcType:addShopItem(itemId, buyPrice, sellPrice) end

_G.NpcType = NpcType  -- Set as global for LSP

return NpcType