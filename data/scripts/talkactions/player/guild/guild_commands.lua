---@type TalkAction
local guildCommands = TalkAction("!guildcommands")

---@param player Player
---@param words string
---@param param string
function guildCommands.onSay(player, words, param)
    if not player then
        return false
    end

    local message = "Available Guild Commands:\n\n"
    
    -- Basic commands available to all players
    message = message .. "Basic Commands:\n"
    message = message .. "!createguild <name> - Create a new guild (costs 10,000 gold, requires level 20)\n"
    message = message .. "!joinguild <name> - Join a guild (requires invitation)\n"
    message = message .. "!leaveguild yes - Leave your current guild\n"
    message = message .. "!guildinfo - Show information about your guild\n"
    message = message .. "!guildcommands - Show this help text\n\n"
    
    -- Check if player is in a guild to show member commands
    local guild = player:getGuild()
    if guild then
        local guildLevel = player:getGuildLevel()
        
        if guildLevel >= 2 then -- Vice-Leader or Leader
            message = message .. "Leadership Commands (Vice-Leader/Leader):\n"
            message = message .. "!inviteguild <player> - Invite a player to the guild\n"
            message = message .. "!revokeguild <player> - Revoke a guild invitation\n"
            message = message .. "!kickguild <player> - Kick a member from the guild\n"
            message = message .. "!guildmotd <message> - Set or view guild message of the day\n"
            message = message .. "!setguildnick <player>, <nick> - Set a guild nickname for a member\n\n"
        end
        
        if guildLevel == 3 then -- Leader only
            message = message .. "Leader Only Commands:\n"
            message = message .. "!promoteguild <player> - Promote a member to vice-leader\n"
            message = message .. "!demoteguild <player> - Demote a vice-leader to member\n"
            message = message .. "!passleadership <player> - Transfer guild leadership\n"
            message = message .. "!disbandguild yes - Disband the guild (permanent!)\n\n"
        end
        
        message = message .. "Guild Level Hierarchy:\n"
        message = message .. "1. Member - Basic guild member\n"
        message = message .. "2. Vice-Leader - Can invite, kick, and manage members\n"
        message = message .. "3. Leader - Full guild control, can promote/demote and disband\n\n"
        
        local playerRank = "Member"
        if guildLevel == 2 then
            playerRank = "Vice-Leader"
        elseif guildLevel == 3 then
            playerRank = "Leader"
        end
        message = message .. string.format("Your current rank in '%s': %s", guild:getName(), playerRank)
    else
        message = message .. "You are not currently in a guild.\n"
        message = message .. "Use !createguild <name> to create a new guild or get invited by existing members."
    end
    
    player:showTextDialog(1950, message) -- Using a scroll item ID for the dialog
    
    return true
end

guildCommands:separator(" ")
guildCommands:register()
