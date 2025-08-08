---@type TalkAction
local guildMotd = TalkAction("!guildmotd")

---@param player Player
---@param words string
---@param param string
function guildMotd.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to change MOTD (leader or vice-leader)
    local guildLevel = player:getGuildLevel()
    if guildLevel < 2 then -- 1 = Member, 2 = Vice-Leader, 3 = Leader
        player:sendCancelMessage("You don't have permission to change the guild message of the day.")
        return false
    end

    -- If no param is provided, show current MOTD
    if param == "" then
        local currentMotd = guild:getMotd()
        if currentMotd == "" then
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Your guild has no message of the day set.")
        else
            player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Current guild MOTD: %s", currentMotd))
        end
        return true
    end

    -- Check MOTD length
    if #param > 200 then
        player:sendCancelMessage("Guild message of the day cannot exceed 200 characters.")
        return false
    end

    -- TODO: Implement guild MOTD setting:
    -- 1. Update guild MOTD in database
    -- 2. Update guild object
    -- 3. Notify all online guild members
    
    local guildName = guild:getName()
    
    -- Set the new MOTD
    if guild:setMotd(param) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Guild message of the day has been changed to: %s", param))
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        
        -- Notify all online guild members about MOTD change
        for _, member in pairs(guild:getMembers()) do
            if member:isOnline() and member ~= player then
                member:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Guild MOTD changed by %s: %s", player:getName(), param))
            end
        end
    else
        player:sendCancelMessage("Failed to update guild message of the day.")
        return false
    end
    
    return true
end

guildMotd:separator(" ")
guildMotd:register()
