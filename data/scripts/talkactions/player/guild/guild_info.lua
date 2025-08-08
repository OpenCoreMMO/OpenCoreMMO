---@type TalkAction
local guildInfo = TalkAction("!guildinfo")

---@param player Player
---@param words string
---@param param string
function guildInfo.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    local guildName = guild:getName()
    local guildMotd = guild:getMotd()
    local memberCount = guild:getMemberCount()
    local onlineCount = guild:getMemberCountOnline()
    
    -- Get player's guild level name
    local guildLevel = player:getGuildLevel()
    local levelName = "Member"
    if guildLevel == 2 then
        levelName = "Vice-Leader"
    elseif guildLevel == 3 then
        levelName = "Leader"
    end

    -- Build guild info message
    local message = string.format("Guild Information:\n")
    message = message .. string.format("Name: %s\n", guildName)
    message = message .. string.format("Your Rank: %s\n", levelName)
    message = message .. string.format("Members: %d (%d online)\n", memberCount, onlineCount)
    
    if guildMotd ~= "" then
        message = message .. string.format("Message of the Day: %s\n", guildMotd)
    else
        message = message .. "Message of the Day: (none)\n"
    end

    -- TODO: Add more guild information:
    -- - Guild creation date
    -- - Guild level/experience
    -- - Guild hall information if applicable
    -- - List of online members
    -- - War status
    
    -- Show online members list if there are any
    if onlineCount > 0 then
        message = message .. "\nOnline Members:\n"
        local onlineMembers = guild:getMembersOnline()
        for i, member in ipairs(onlineMembers) do
            local memberLevel = member:getGuildLevel()
            local memberRank = "Member"
            if memberLevel == 2 then
                memberRank = "Vice-Leader"
            elseif memberLevel == 3 then
                memberRank = "Leader"
            end
            message = message .. string.format("- %s (%s)\n", member:getName(), memberRank)
        end
    end
    
    player:showTextDialog(1950, message) -- Using a scroll item ID for the dialog
    
    return true
end

guildInfo:separator(" ")
guildInfo:register()
