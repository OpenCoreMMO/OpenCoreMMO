---@type TalkAction
local revokeGuild = TalkAction("!revokeguild")

---@param player Player
---@param words string
---@param param string
function revokeGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to revoke invitations (leader or vice-leader)
    local guildLevel = player:getGuildLevel()
    if guildLevel < 2 then -- 1 = Member, 2 = Vice-Leader, 3 = Leader
        player:sendCancelMessage("You don't have permission to revoke guild invitations.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the player whose invitation you want to revoke.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as revoker
    if targetPlayer == player then
        player:sendCancelMessage("You cannot revoke your own invitation.")
        return false
    end

    -- Check if target has an invitation from this guild and revoke it
    local guildName = guild:getName()
    
    if guild:revokeInvitation(targetPlayer) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have revoked %s's invitation to join %s.", targetPlayer:getName(), guildName))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Your invitation to join the guild '%s' has been revoked by %s.", guildName, player:getName()))
        
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
        targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
    else
        player:sendCancelMessage(string.format("%s does not have an invitation to your guild.", targetPlayer:getName()))
        return false
    end
    
    return true
end

revokeGuild:separator(" ")
revokeGuild:register()
