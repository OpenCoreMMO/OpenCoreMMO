---@type TalkAction
local leaveGuild = TalkAction("!leaveguild")

---@param player Player
---@param words string
---@param param string
function leaveGuild.onSay(player, words, param)
    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You are not in a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if player is the guild leader
    local guildLevel = player:getGuildLevel()
    local guildRank = player:getGuildRank()
    
    -- Leaders cannot leave guild, they must disband it or transfer leadership first
    local isLeader = false
    if guildLevel and guildLevel >= 3 then
        isLeader = true
    elseif guildRank and string.lower(guildRank):find("leader") then
        isLeader = true
    end
    
    if isLeader then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "As guild leader, you cannot leave the guild. You must either transfer leadership or disband the guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- No confirmation needed, direct leave
    local guildName = guild:getName()
    local playerName = player:getName()
    local guildId = guild:getId()
    
    -- Force close guild channel BEFORE removing player from guild
    -- This ensures the guild chat window is properly closed on the client
    
    -- Try to trigger guild channel close by getting current guild channel
    -- We need to do this before setGuild(nil) because after that we lose access to guild channel
    
    -- Remove player from guild (this should trigger channel events)
    player:setGuild(nil)
    
    -- Notify the player
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You have left the guild '" .. guildName .. "'.")
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    
    -- Notify other guild members who are online
    local allPlayers = Game.getPlayers()
    local notifiedCount = 0
    for _, otherPlayer in pairs(allPlayers) do
        local otherGuild = otherPlayer:getGuild()
        if otherGuild and otherGuild:getId() == guildId and otherPlayer ~= player then
            -- Send personal notification only (sendChannelMessage doesn't exist)
            otherPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, playerName .. " has left the guild.")
            notifiedCount = notifiedCount + 1
        end
    end
    
    return true
end

leaveGuild:separator(" ")
leaveGuild:register()
