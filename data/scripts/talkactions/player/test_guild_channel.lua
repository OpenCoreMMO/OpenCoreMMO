---@type TalkAction
local testCreateGuildWithChannel = TalkAction("!testguildchannel")

---@param player Player
---@param words string
---@param param string
function testCreateGuildWithChannel.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Testing guild creation with channel!")

    if not param or param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !testguildchannel <guild name>")
        return true
    end

    player:sendTextMessage(MESSAGE_INFO_DESCR, "Creating guild: " .. param)

    -- Check if player already has a guild
    local guild = player:getGuild()
    if guild then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "You are already in a guild: " .. guild:getName())
        return true
    end

    -- Create new guild using Guild() function
    local newGuild = Guild(param)
    if newGuild then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "SUCCESS: Guild created!")
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild ID: " .. newGuild:getId())
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild Name: " .. newGuild:getName())

        -- Set the player as guild leader
        player:setGuild(newGuild)

        -- Check if the guild channel was created
        local updatedGuild = player:getGuild()
        if updatedGuild then
            player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You are now the leader of " .. updatedGuild:getName() .. "!")
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild channel should be available in your channel list.")
        else
            player:sendTextMessage(MESSAGE_INFO_DESCR, "Error: Could not join the created guild")
        end
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "FAILURE: Could not create guild")
    end

    return true
end

testCreateGuildWithChannel:separator(" ")
testCreateGuildWithChannel:register()
