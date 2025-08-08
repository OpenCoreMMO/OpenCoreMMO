---@type TalkAction
local simpleTest = TalkAction("!simpletest")

---@param player Player
---@param words string
---@param param string
function simpleTest.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Simple test is working!")
    return true
end

simpleTest:separator(" ")
simpleTest:register()
