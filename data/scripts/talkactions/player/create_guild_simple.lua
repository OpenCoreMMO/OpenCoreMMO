---@type TalkAction
local createGuildSimple = TalkAction("!createguildsimple")

---@param player Player
---@param words string
---@param param string
function createGuildSimple.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Simple create guild command executed!")

    if not param or param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !createguildsimple <guild name>")
        return true
    end

    player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild name would be: " .. param)

    -- Check if player already has a guild
    local guild = player:getGuild()
    if guild then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "You are already in a guild.")
        return true
    end

    player:sendTextMessage(MESSAGE_INFO_DESCR, "You are not in a guild - ready to create!")

    return true
end

createGuildSimple:separator(" ")
createGuildSimple:register()
