-- Simple guild configuration inline
local CREATION_COST = 10000
local MIN_LEVEL_CREATE = 20
local MIN_GUILD_NAME_LENGTH = 4
local MAX_GUILD_NAME_LENGTH = 29

---@type TalkAction
local createGuild2 = TalkAction("!createguild2")

---@param player Player
---@param words string
---@param param string
function createGuild2.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Create guild command executed!")

    if not param or param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !createguild2 <guild name>")
        return true
    end

    -- Basic validation
    if #param < MIN_GUILD_NAME_LENGTH then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Guild name must be at least %d characters.", MIN_GUILD_NAME_LENGTH))
        return true
    end

    if #param > MAX_GUILD_NAME_LENGTH then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Guild name cannot exceed %d characters.", MAX_GUILD_NAME_LENGTH))
        return true
    end

    -- Check if player already has a guild
    local guild = player:getGuild()
    if guild then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "You are already in a guild.")
        return true
    end

    -- Basic level check
    if player:getLevel() < MIN_LEVEL_CREATE then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("You need to be at least level %d to create a guild.", MIN_LEVEL_CREATE))
        return true
    end

    -- Check money
    if not player:hasMoneyBank(CREATION_COST) then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("You need %d gold in your bank to create a guild.", CREATION_COST))
        return true
    end

    -- Simulate guild creation
    player:removeMoneyBank(CREATION_COST)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Guild '%s' would be created! Cost: %d gold removed from bank.", param, CREATION_COST))

    return true
end

createGuild2:separator(" ")
createGuild2:register()
