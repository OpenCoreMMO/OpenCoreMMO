---@type TalkAction
local leaveGuild = TalkAction("!leaveguild")

---@param player Player
---@param words string
---@param param string
function leaveGuild.onSay(player, words, param)
    local guild = player:getGuild()
    if not guild then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "You are not in a guild.")
        return true
    end
    
    local guildName = guild:getName()
    
    -- TODO: Check if player is guild leader and handle differently
    -- local guildLevel = player:getGuildLevel()
    -- if guildLevel == 3 then -- Leader
    --     player:sendTextMessage(MESSAGE_INFO_DESCR, "You cannot leave the guild as a leader. Transfer leadership or disband the guild.")
    --     return true
    -- end
    
    -- TODO: Implement actual guild leaving via C# functions
    -- player:setGuild(nil)
    -- guild:removeMember(player)
    
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have left the guild '%s'.", guildName))
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
    
    return true
end

leaveGuild:separator(" ")
leaveGuild:register()
