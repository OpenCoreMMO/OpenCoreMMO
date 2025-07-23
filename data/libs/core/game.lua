function Game.broadcastMessage(message, messageType)
    if not messageType then
        messageType = MESSAGE_STATUS_WARNING
    end

---@diagnostic disable-next-line: missing-parameter
    for _, player in ipairs(Game.getPlayers()) do
        player:sendTextMessage(messageType, message)
    end
end