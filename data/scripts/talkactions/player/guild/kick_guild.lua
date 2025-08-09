---@type TalkAction
local kickGuild = TalkAction("!kickguild")

---@param player Player
---@param words string
---@param param string
function kickGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to kick members (leader or vice-leader)
    local guildLevel = player:getGuildLevel()
    if guildLevel < 2 then -- 1 = Member, 2 = Vice-Leader, 3 = Leader
        player:sendCancelMessage("You don't have permission to kick members from the guild.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the member you want to kick.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as kicker
    if targetPlayer == player then
        player:sendCancelMessage("You cannot kick yourself from the guild.")
        return false
    end

    -- Check if target is in the same guild
    local targetGuild = targetPlayer:getGuild()
    if not targetGuild or targetGuild:getId() ~= guild:getId() then
        player:sendCancelMessage(string.format("%s is not in your guild.", targetPlayer:getName()))
        return false
    end

    -- Check permissions - vice-leaders cannot kick other vice-leaders or leaders
    local targetGuildLevel = targetPlayer:getGuildLevel()
    if guildLevel == 2 and targetGuildLevel >= 2 then -- Vice-leader trying to kick vice-leader or leader
        player:sendCancelMessage("You cannot kick other vice-leaders or the guild leader.")
        return false
    end

    -- Leaders cannot kick other leaders (should transfer leadership first)
    if targetGuildLevel == 3 then -- Target is leader
        player:sendCancelMessage("You cannot kick the guild leader. Transfer leadership first.")
        return false
    end

    -- Remove player from guild
    local guildName = guild:getName()
    
    if guild:removeMember(targetPlayer) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have kicked %s from %s.", targetPlayer:getName(), guildName))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have been kicked from the guild '%s' by %s.", guildName, player:getName()))
        
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
        targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
        
        -- Notify other guild members (using getMembersOnline to avoid isOnline issues)
        local membersOnline = guild:getMembersOnline()
        for i, member in pairs(membersOnline) do
            if member and member ~= player and member ~= targetPlayer then
                member:sendTextMessage(MESSAGE_INFO_DESCR, string.format("%s has been kicked from the guild by %s.", targetPlayer:getName(), player:getName()))
            end
        end
    else
        player:sendCancelMessage("Failed to kick member. Please try again.")
        return false
    end
    
    return true
end

kickGuild:separator(" ")
kickGuild:register()
