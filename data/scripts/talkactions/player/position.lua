local talkAction = TalkAction("!position")

function talkAction.onSay(player, words, param)
    local position = player:getPosition()
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Your current position is: " .. position.x .. ", " .. position.y .. ", " .. position.z .. ".")
    return true
end

talkAction:register()
