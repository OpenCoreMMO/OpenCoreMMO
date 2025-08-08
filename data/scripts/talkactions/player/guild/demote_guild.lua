---@type TalkAction
local demoteGuild = TalkAction("!demoteguild")

---@param player Player
---@param words string
---@param param string
function demoteGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to demote (only leaders)
    local guildLevel = player:getGuildLevel()
    if guildLevel ~= 3 then -- Only leaders can demote
        player:sendCancelMessage("Only the guild leader can demote members.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the member you want to demote.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as demoter
    if targetPlayer == player then
        player:sendCancelMessage("You cannot demote yourself.")
        return false
    end

    -- Check if target is in the same guild
    local targetGuild = targetPlayer:getGuild()
    if not targetGuild or targetGuild:getId() ~= guild:getId() then
        player:sendCancelMessage(string.format("%s is not in your guild.", targetPlayer:getName()))
        return false
    end

    -- Check target's current level and demote accordingly
    local targetGuildLevel = targetPlayer:getGuildLevel()
    local newLevel = targetGuildLevel
    local levelName = ""
    
    if targetGuildLevel == 2 then -- Vice-Leader -> Member
        newLevel = 1
        levelName = "Member"
    elseif targetGuildLevel == 1 then -- Already member
        player:sendCancelMessage(string.format("%s is already a Member. Cannot demote further.", targetPlayer:getName()))
        return false
    else -- Leader
        player:sendCancelMessage(string.format("%s is the guild leader. Cannot demote the leader.", targetPlayer:getName()))
        return false
    end

    -- Demote the member
    local guildName = guild:getName()
    
    if guild:demoteMember(targetPlayer, newLevel) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have demoted %s to %s in %s.", targetPlayer:getName(), levelName, guildName))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have been demoted to %s in the guild '%s' by %s.", levelName, guildName, player:getName()))
        
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
        targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
        
        -- Notify other guild members
        for _, member in pairs(guild:getMembers()) do
            if member:isOnline() and member ~= player and member ~= targetPlayer then
                member:sendTextMessage(MESSAGE_INFO_DESCR, string.format("%s has been demoted to %s by %s.", targetPlayer:getName(), levelName, player:getName()))
            end
        end
    else
        player:sendCancelMessage("Failed to demote member. Please try again.")
        return false
    end
    
    return true
end

demoteGuild:separator(" ")
demoteGuild:register()
