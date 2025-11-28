---@type TalkAction
local testCreateGuild = TalkAction("!testcreate")

---@param player Player
---@param words string
---@param param string
function testCreateGuild.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Test create guild command working!")

    if not param or param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !testcreate <guild name>")
        return true
    end

    player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild name: " .. param)

    -- Test the Guild() function directly
    player:sendTextMessage(MESSAGE_INFO_DESCR, "Attempting to create guild using Guild() function...")

    local newGuild = Guild(param)
    if newGuild then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "SUCCESS: Guild created with Guild() function!")
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild ID: " .. newGuild:getId())
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild Name: " .. newGuild:getName())
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "FAILURE: Guild() function returned nil")
    end

    return true
end

testCreateGuild:separator(" ")
testCreateGuild:register()
