---@type TalkAction
local testGuild = TalkAction("!testguild")

---@param player Player
---@param words string
---@param param string
function testGuild.onSay(player, words, param)
    if not player then
        return false
    end

    local message = "Guild System Test Results:\n\n"

    -- Test basic player functions
    message = message .. string.format("Player: %s\n", player:getName())
    message = message .. string.format("Level: %d\n", player:getLevel())

    -- Test guild functions
    local guild = player:getGuild()
    if guild then
        message = message .. string.format("Guild: %s (ID: %d)\n", guild:getName(), guild:getId())
        message = message .. string.format("Guild Level: %d\n", player:getGuildLevel())
        message = message .. string.format("Guild Rank: %s\n", player:getGuildRank())
        message = message .. string.format("Guild Nick: '%s'\n", player:getGuildNick())
        message = message .. string.format("Guild MOTD: '%s'\n", guild:getMotd())
        message = message .. string.format("Members Online: %d\n", guild:getMemberCountOnline())
        message = message .. string.format("Total Members: %d\n", guild:getMemberCount())
    else
        message = message .. "Guild: None\n"
        message = message .. "Guild Level: 0\n"
    end

    -- Test bank functions
    local hasMoney = player:hasMoneyBank(1000)
    message = message .. string.format("Has 1000 gold in bank: %s\n", hasMoney and "Yes" or "No")

    -- Test configuration
    message = message .. "\nGuild Configuration:\n"
    if GuildConfig then
        message = message .. string.format("Creation Cost: %d gold\n", GuildConfig.CREATION_COST)
        message = message .. string.format("Min Level to Create: %d\n", GuildConfig.MIN_LEVEL_CREATE)
        message = message .. string.format("Min Level to Join: %d\n", GuildConfig.MIN_LEVEL_JOIN)
    else
        message = message .. "Guild configuration not loaded!\n"
    end

    -- Function availability test
    message = message .. "\nFunction Tests:\n"
    local functions_ok = true

    -- Test if functions exist
    local test_functions = {
        "getGuild", "getGuildLevel", "getGuildId", "getGuildNick",
        "setGuildNick", "removeMoneyBank", "hasMoneyBank"
    }

    for _, func_name in ipairs(test_functions) do
        if player[func_name] then
            message = message .. string.format("✓ %s: Available\n", func_name)
        else
            message = message .. string.format("✗ %s: Missing\n", func_name)
            functions_ok = false
        end
    end

    if functions_ok then
        message = message .. "\n✅ All guild functions are available!"
    else
        message = message .. "\n❌ Some guild functions are missing!"
    end

    player:showTextDialog(1950, message)

    return true
end

testGuild:separator(" ")
testGuild:register()
