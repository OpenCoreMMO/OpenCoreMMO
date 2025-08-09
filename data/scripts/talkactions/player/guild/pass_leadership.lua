---@type TalkAction
local passLeadership = TalkAction("!passleadership")

---@param player Player
---@param words string
---@param param string
function passLeadership.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player is the guild leader
    if player:getGuildLevel() ~= 3 then -- Only leaders can pass leadership
        player:sendCancelMessage("Only the guild leader can pass leadership.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the member you want to pass leadership to.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as current leader
    if targetPlayer == player then
        player:sendCancelMessage("You are already the guild leader.")
        return false
    end

    -- Check if target is in the same guild
    local targetGuild = targetPlayer:getGuild()
    if not targetGuild or targetGuild:getId() ~= guild:getId() then
        player:sendCancelMessage(string.format("%s is not in your guild.", targetPlayer:getName()))
        return false
    end

    -- Check if target is at least a member (not invited)
    local targetGuildLevel = targetPlayer:getGuildLevel()
    if targetGuildLevel < 1 then
        player:sendCancelMessage(string.format("%s is not a member of the guild.", targetPlayer:getName()))
        return false
    end

    -- Transfer leadership:
    -- Use the dedicated transferLeadership function
    
    local guildName = guild:getName()
    
    -- Transfer leadership using the dedicated function
    if not guild:transferLeadership(player, targetPlayer) then
        player:sendCancelMessage("Failed to transfer leadership. Please try again.")
        return false
    end
    
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have passed leadership of %s to %s. You are now a Vice-Leader.", guildName, targetPlayer:getName()))
    targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have received leadership of the guild '%s' from %s. You are now the Guild Leader.", guildName, player:getName()))
    
    -- Notify all online guild members about leadership change
    local membersOnline = guild:getMembersOnline()
    for i, member in pairs(membersOnline) do
        if member and member ~= player and member ~= targetPlayer then
            member:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Guild leadership of '%s' has been passed from %s to %s.", guildName, player:getName(), targetPlayer:getName()))
        end
    end
    
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    
    return true
end

passLeadership:separator(" ")
passLeadership:register()
