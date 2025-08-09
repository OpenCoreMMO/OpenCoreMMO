-- Guild channel MOTD display on login
local guildMotdOnLogin = CreatureEvent("GuildMotdOnLogin")

function guildMotdOnLogin.onLogin(player)
    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        return true
    end

    -- Get guild MOTD
    local motd = guild:getMotd()
    if motd and motd ~= "" then
        -- Send MOTD to guild channel when player logs in
        addEvent(function(playerId)
            local loginPlayer = Player(playerId)
            if loginPlayer then
                local playerGuild = loginPlayer:getGuild()
                if playerGuild then
                    -- Get guild channel ID (channel ID 0 is the guild channel)
                    local guildChannelId = 0
                    
                    -- Send MOTD directly to guild channel without the "Guild MOTD:" prefix
                    -- Use white text (TALKTYPE_CHANNEL_Y = 7 for white text in guild channel)
                    sendChannelMessage(guildChannelId, 7, motd)
                end
            end
        end, 3000, player:getId())
    end

    return true
end

guildMotdOnLogin:register()
