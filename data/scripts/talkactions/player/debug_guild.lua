---@type TalkAction
local debugGuild = TalkAction("!debugguild")

---@param player Player
---@param words string
---@param param string
function debugGuild.onSay(player, words, param)
    player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Debug guild command working!")

    -- Test basic player functions
    local playerName = player:getName()
    local playerLevel = player:getLevel()

    player:sendTextMessage(MESSAGE_INFO_DESCR, "Player: " .. playerName .. " (Level " .. playerLevel .. ")")

    -- Test hasMoneyBank function
    local hasMoney = player:hasMoneyBank(1000)
    player:sendTextMessage(MESSAGE_INFO_DESCR, "Has 1000 gold in bank: " .. tostring(hasMoney))

    -- Test getGuild function
    local guild = player:getGuild()
    if guild then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Player is in a guild")
    else
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Player is not in a guild")
    end

    return true
end

debugGuild:separator(" ")
debugGuild:register()
