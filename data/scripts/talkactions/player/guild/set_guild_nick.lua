---@type TalkAction
local setGuildNick = TalkAction("!setguildnick")

---@param player Player
---@param words string
---@param param string
function setGuildNick.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player has permission to set guild nicks (leader or vice-leader)
    local guildLevel = player:getGuildLevel()
    if guildLevel < 2 then
        -- 1 = Member, 2 = Vice-Leader, 3 = Leader
        player:sendCancelMessage("You don't have permission to set guild nicknames.")
        return false
    end

    -- Parse parameters: player name and nick
    local params = param:split(",")
    if #params < 2 then
        player:sendCancelMessage("Usage: !setguildnick player_name, nickname")
        return false
    end

    local targetName = params[1]:trim()
    local guildNick = params[2]:trim()

    -- Check if target player name is provided
    if targetName == "" then
        player:sendCancelMessage("Please enter the name of the player.")
        return false
    end

    -- Find the target player
    local targetPlayer = Player(targetName)
    if not targetPlayer then
        player:sendCancelMessage("Player not found or not online.")
        return false
    end

    -- Check if target is in the same guild
    local targetGuild = targetPlayer:getGuild()
    if not targetGuild or targetGuild:getId() ~= guild:getId() then
        player:sendCancelMessage(string.format("%s is not in your guild.", targetPlayer:getName()))
        return false
    end

    -- Check guild nick length
    if guildNick ~= "" and (#guildNick < 2 or #guildNick > 20) then
        player:sendCancelMessage("Guild nickname must be between 2 and 20 characters, or empty to remove.")
        return false
    end

    -- Check if guild nick contains only valid characters
    if guildNick ~= "" and not guildNick:match("^[%w%s]+$") then
        player:sendCancelMessage("Guild nickname contains invalid characters.")
        return false
    end

    -- TODO: Implement guild nick setting:
    -- 1. Update target player's guild nick in database
    -- 2. Update target player's guild nick in game
    -- 3. Update player's display name/title

    if guildNick == "" then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have removed %s's guild nickname.", targetPlayer:getName()))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Your guild nickname has been removed by %s.", player:getName()))
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have set %s's guild nickname to '%s'.", targetPlayer:getName(), guildNick))
        targetPlayer:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Your guild nickname has been set to '%s' by %s.", guildNick, player:getName()))
    end

    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    targetPlayer:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)

    return true
end

setGuildNick:separator(" ")
setGuildNick:register()
