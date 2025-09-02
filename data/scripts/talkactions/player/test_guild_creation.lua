---@type TalkAction
local testGuildCreation = TalkAction("!testguildcreation")

---@param player Player
---@param words string
---@param param string
function testGuildCreation.onSay(player, words, param)
    print("DEBUG: !testguildcreation called by " .. player:getName())
    
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Test command working!")
    
    if param and param ~= "" then
        print("DEBUG: Testing Guild() function with: " .. param)
        local testGuild = Guild(param)
        if testGuild then
            player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild() function works! Created: " .. testGuild:getName())
        else
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild() function failed")
        end
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !testguildcreation <name>")
    end
    
    return true
end

testGuildCreation:separator(" ")
testGuildCreation:register()
