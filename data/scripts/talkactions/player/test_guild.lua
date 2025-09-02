---@type TalkAction
local testGuild = TalkAction("!testguild")

---@param player Player
---@param words string
---@param param string
function testGuild.onSay(player, words, param)
    -- Very basic test to see if command works at all
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild command executed successfully!")
    
    -- Test if player:getGuild() function exists
    local hasGuildFunction = type(player.getGuild) == "function"
    player:sendTextMessage(MESSAGE_INFO_DESCR, "Has getGuild function: " .. tostring(hasGuildFunction))
    
    -- Try to call it safely
    local success, guild = pcall(function() return player:getGuild() end)
    
    if success then
        if guild then
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Player has guild (not nil)")
        else
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Player has no guild (nil)")
        end
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Error calling getGuild: " .. tostring(guild))
    end
    
    return true
end

testGuild:separator(" ")
testGuild:register()
