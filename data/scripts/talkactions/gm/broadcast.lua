---@type TalkAction
local talkAction = TalkAction("/b")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    if not player:getGroup():getAccess() then
        return true
    end

    if param == "" then
        player:sendCancelMessage("Usage: /b message")
        return true
    end

    local playerName = player:getName()
    local message = param

    -- Log to server with sender name
    logger.info(string.format("[BROADCAST] %s: %s", playerName, message))

    -- Broadcast to all players with sender name
    local broadcastMessage = string.format("[%s] %s", playerName, message)
    Game.broadcastMessage(broadcastMessage, MESSAGE_INFO_DESCR)

    return true
end

talkAction:separator(" ")
talkAction:register()