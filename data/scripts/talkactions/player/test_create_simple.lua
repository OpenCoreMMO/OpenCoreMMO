---@type TalkAction
local testCreate = TalkAction("!testcreate")

---@param player Player
---@param words string
---@param param string
function testCreate.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Create guild test command works!")

    if param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "No guild name provided")
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild name would be: " .. param)
    end

    return true
end

testCreate:separator(" ")
testCreate:register()
