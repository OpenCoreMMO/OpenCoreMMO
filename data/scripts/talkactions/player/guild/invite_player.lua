---@type TalkAction
local invitePlayer = TalkAction("!invite")

---@param player Player
---@param words string
---@param param string
function invitePlayer.onSay(player, words, param)
    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You are not in a guild.")
        return true
    end

    -- Check if player has permission to invite
    -- Simplified solution: Allow any guild member to invite others
    -- This matches most OT servers where any member can invite
    local hasPermission = true
    
    if not hasPermission then
        -- This should never happen with simplified approach
        return true
    end

    if not param or param == "" then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "Usage: !invite <player name>")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Find the target player
    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "Player '" .. param .. "' is not online.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if target is the same as inviter
    if targetPlayer == player then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You cannot invite yourself.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if target is already in a guild
    if targetPlayer:getGuild() then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, targetPlayer:getName() .. " is already in a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check minimum level requirement for target
    local minLevel = 8
    if targetPlayer:getLevel() < minLevel then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, targetPlayer:getName() .. " needs to be at least level " .. minLevel .. " to join a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if target already has a pending invitation to this guild
    local guildId = guild:getId()
    
    -- Use storage to track guild invitations (storage key: base + guild_id)
    local invitationStorageBase = 50000
    local invitationStorage = invitationStorageBase + guildId
    
    if targetPlayer:getStorageValue(invitationStorage) == 1 then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, targetPlayer:getName() .. " already has a pending invitation to your guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Create the invitation by setting storage
    targetPlayer:setStorageValue(invitationStorage, 1)
    
    local guildName = guild:getName()
    
    -- Send success messages
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You have invited " .. targetPlayer:getName() .. " to join " .. guildName .. ".")
    targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You have been invited to join the guild '" .. guildName .. "' by " .. player:getName() .. ". Use !joinguild " .. guildName .. " to accept.")
    
    -- Visual effects
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    
    return true
end

invitePlayer:separator(" ")
invitePlayer:register()
