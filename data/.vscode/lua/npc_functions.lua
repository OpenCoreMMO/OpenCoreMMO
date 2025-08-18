---@diagnostic disable: missing-return
---@diagnostic disable: undefined-doc-name
---@diagnostic disable: inject-field
---@diagnostic disable: assign-type-mismatch

---@class Npc : Creature
Npc = {}

---Checks if the object is an NPC.
---@return boolean
function Npc:isNpc() end

---Sets player interaction with the NPC.
---@param player Player
---@param topic number|nil
---@return boolean
function Npc:setPlayerInteraction(player, topic) end

---Removes player interaction with the NPC.
---@param player Player
---@return boolean
function Npc:removePlayerInteraction(player) end

---Checks if the NPC is interacting with a player.
---@param player Player
---@return boolean
function Npc:isInteractingWithPlayer(player) end

---Checks if the NPC is in talk range of a position.
---@param position table
---@param range number|nil
---@return boolean
function Npc:isInTalkRange(position, range) end

---Checks if the player is interacting with the NPC on a specific topic.
---@param player Player
---@param topicId number|nil
---@return boolean
function Npc:isPlayerInteractingOnTopic(player, topicId) end

---Opens the shop window for a player.
---@param player Player
---@param topicId number|nil
---@return boolean
function Npc:openShopWindow(player, topicId) end

---Opens the shop window for a player with a table of items.
---@param player Player
---@param items table
---@return boolean
function Npc:openShopWindowTable(player, items) end

---Closes the shop window for a player.
---@param player Player
---@param topicId number|nil
---@return boolean
function Npc:closeShopWindow(player, topicId) end

---Checks if the NPC is a merchant.
---@return boolean
function Npc:isMerchant() end

_G.Npc = Npc  -- Set as global for LSP

return Npc