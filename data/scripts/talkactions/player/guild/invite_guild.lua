---@type TalkAction
local inviteGuild = TalkAction("!inviteguild")

---@param player Player
---@param words string
---@param param string
function inviteGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to invite (leader or vice-leader)
    local guildLevel = player:getGuildLevel()
    if guildLevel < 2 then
        -- 1 = Member, 2 = Vice-Leader, 3 = Leader
        player:sendCancelMessage("You don't have permission to invite players to the guild.")
        return false
    end

    -- Check if target player name is provided
    if param == "" then
        player:sendCancelMessage("Please enter the name of the player you want to invite.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is the same as inviter
    if targetPlayer == player then
        player:sendCancelMessage("You cannot invite yourself.")
        return false
    end

    -- Check if target is already in a guild
    if targetPlayer:getGuild() then
        player:sendCancelMessage(string.format("%s is already in a guild.", targetPlayer:getName()))
        return false
    end

    -- Check minimum level requirement for target
    local minLevel = 8
    if targetPlayer:getLevel() < minLevel then
        player:sendCancelMessage(string.format("%s needs to be at least level %d to join a guild.", targetPlayer:getName(), minLevel))
        return false
    end

    -- Send guild invitation
    local guildName = guild:getName()

    if guild:invitePlayer(targetPlayer) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have invited %s to join %s.", targetPlayer:getName(), guildName))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have been invited to join the guild '%s' by %s. Use '!joinguild %s' to accept.", guildName, player:getName(), guildName))

        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    else
        player:sendCancelMessage("Failed to send invitation. Please try again.")
        return false
    end

    return true
end

inviteGuild:separator(" ")
inviteGuild:register()
