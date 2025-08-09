---@type TalkAction
local joinGuild = TalkAction("!joinguild")

---@param player Player
---@param words string
---@param param string
function joinGuild.onSay(player, words, param)
    if not param or param == "" then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "Usage: !joinguild <guild name>")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if player is already in a guild
    if player:getGuild() then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You are already in a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check minimum level requirement
    local minLevel = 8
    if player:getLevel() < minLevel then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You need to be at least level " .. minLevel .. " to join a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- First, we need to check if the player has an invitation for any guild with this name
    -- We'll iterate through possible guild IDs to find the invitation
    local foundGuildId = nil
    local foundInvitation = false
    local invitationStorageBase = 50000
    
    -- Check storage values for guild invitations (try a reasonable range)
    for guildId = 1, 100 do -- Adjust range as needed
        local invitationStorage = invitationStorageBase + guildId
        if player:getStorageValue(invitationStorage) == 1 then
            -- Player has an invitation for guild with this ID
            foundInvitation = true
            foundGuildId = guildId
            -- Remove the invitation from storage
            player:setStorageValue(invitationStorage, -1)
            break
        end
    end
    
    if not foundInvitation then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "You don't have an invitation to join '" .. param .. "'. Ask a leader or vice-leader to invite you.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Now try to find existing guild by using Game.getPlayers() function
    local guild = nil
    
    -- Try to get guild from the game world by iterating through all online players
    local allPlayers = Game.getPlayers()
    for _, onlinePlayer in pairs(allPlayers) do
        local playerGuild = onlinePlayer:getGuild()
        if playerGuild and playerGuild:getName():lower() == param:lower() and playerGuild:getId() == foundGuildId then
            guild = playerGuild
            break
        end
    end
    
    if not guild then
        -- If we can't find it through players, the guild might be empty
        -- We'll need to handle this case differently
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "Unable to find the guild '" .. param .. "'. The guild might be empty or not exist.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end

    -- Check if guild has space (max 256 members in Tibia)
    local memberCount = guild:getMemberCount()
    local maxMembers = 256
    if memberCount >= maxMembers then
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "The guild '" .. param .. "' is full.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Join the guild
    if player:setGuild(guild) then
        local guildName = guild:getName()
        
        -- Send success message to player
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Welcome to " .. guildName .. "! You have successfully joined the guild.")
        
        -- Show guild MOTD to new member in guild channel
        local motd = guild:getMotd()
        if motd and motd ~= "" then
            addEvent(function(playerId)
                local joinedPlayer = Player(playerId)
                if joinedPlayer then
                    local playerGuild = joinedPlayer:getGuild()
                    if playerGuild then
                        -- Send MOTD directly to guild channel without the "Guild MOTD:" prefix
                        -- Use white text (TALKTYPE_CHANNEL_Y = 7 for white text in guild channel)
                        local guildChannelId = 0
                        sendChannelMessage(guildChannelId, 7, motd)
                    end
                end
            end, 1000, player:getId())
        end
        
        -- Notify other guild members by iterating through online players
        local allPlayers = Game.getPlayers()
        for _, onlinePlayer in pairs(allPlayers) do
            local playerGuild = onlinePlayer:getGuild()
            if playerGuild and playerGuild:getId() == guild:getId() and onlinePlayer ~= player then
                onlinePlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, player:getName() .. " has joined the guild.")
            end
        end
        
        -- Visual effects
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    else
        player:sendTextMessage(MESSAGE_STATUS_WARNING, "Failed to join the guild. Please try again later.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
    end
    
    return true
end

joinGuild:separator(" ")
joinGuild:register()
