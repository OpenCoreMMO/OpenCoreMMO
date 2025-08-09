---@type TalkAction
local promoteGuild = TalkAction("!promoteguild")

---@param player Player
---@param words string
---@param param string
function promoteGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to promote (only leaders)
    local guildLevel = player:getGuildLevel()
    if guildLevel ~= 3 then -- Only leaders can promote
        player:sendCancelMessage("Only the guild leader can promote members.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the member you want to promote.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as promoter
    if targetPlayer == player then
        player:sendCancelMessage("You cannot promote yourself.")
        return false
    end

    -- Check if target is in the same guild
    local targetGuild = targetPlayer:getGuild()
    if not targetGuild or targetGuild:getId() ~= guild:getId() then
        player:sendCancelMessage(string.format("%s is not in your guild.", targetPlayer:getName()))
        return false
    end

    -- Check target's current level and promote accordingly
    local targetGuildLevel = targetPlayer:getGuildLevel()
    local newLevel = targetGuildLevel
    local levelName = ""
    
    if targetGuildLevel == 1 then -- Member -> Vice-Leader
        newLevel = 2
        levelName = "Vice-Leader"
    elseif targetGuildLevel == 2 then -- Vice-Leader -> cannot promote further
        player:sendCancelMessage(string.format("%s is already a Vice-Leader. Cannot promote further.", targetPlayer:getName()))
        return false
    else -- Already leader
        player:sendCancelMessage(string.format("%s is already the guild leader.", targetPlayer:getName()))
        return false
    end

    -- Promote the member
    local guildName = guild:getName()
    
    if guild:promoteMember(targetPlayer, newLevel) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have promoted %s to %s in %s.", targetPlayer:getName(), levelName, guildName))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have been promoted to %s in the guild '%s' by %s.", levelName, guildName, player:getName()))
        
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        
        -- Notify other guild members
        local members = guild:getMembers()
        for _, member in pairs(members) do
            if member:isOnline() and member ~= player and member ~= targetPlayer then
                member:sendTextMessage(MESSAGE_INFO_DESCR, string.format("%s has been promoted to %s by %s.", targetPlayer:getName(), levelName, player:getName()))
            end
        end
    else
        player:sendCancelMessage("Failed to promote member. Please try again.")
        return false
    end
    
    return true
end

promoteGuild:separator(" ")
promoteGuild:register()
