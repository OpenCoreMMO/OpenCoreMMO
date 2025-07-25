---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Game
Game = {}

---Creates a new NPC type.
---@param npcType string
function Game:createNpcType(npcType) end

---Gets the return message for a given value.
---@param value number
---@return string
function Game:getReturnMessage(value) end

---Creates an item.
---@param itemId number|string
---@param count number|nil
---@param position table|nil
---@return Item|table|nil
function Game:createItem(itemId, count, position) end

---Creates a monster.
---@param monsterName string
---@param position table
---@param extended boolean|nil
---@param force boolean|nil
---@param master table|nil
---@return Monster|nil
function Game:createMonster(monsterName, position, extended, force, master) end

---Creates an NPC.
---@param npcName string
---@param position table
---@param extended boolean|nil
---@param force boolean|nil
---@return Npc|nil
function Game:createNpc(npcName, position, extended, force) end

---Reloads game data.
---@param reloadType number
---@return boolean
function Game:reload(reloadType) end

---Gets all players.
---@return table
function Game:getPlayers() end

---Gets the normalized player name.
---@param name string
---@param isNewName boolean|nil
---@return string|nil
function Game:getNormalizedPlayerName(name, isNewName) end

_G.Game = Game  -- Set as global for LSP

return Game