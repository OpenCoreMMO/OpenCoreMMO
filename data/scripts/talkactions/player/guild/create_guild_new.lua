-- Guild configuration
local CREATION_COST = 10000
local MIN_LEVEL_CREATE = 20
local MIN_GUILD_NAME_LENGTH = 4
local MAX_GUILD_NAME_LENGTH = 29

---@type TalkAction
local createGuild = TalkAction("!createguild")

---@param player Player
---@param words string
---@param param string
function createGuild.onSay(player, words, param)
    print("DEBUG: !createguild command called")
    print("DEBUG: param = " .. tostring(param))
    
    if not param or param == "" then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !createguild <guild name>")
        return true
    end
    
    print("DEBUG: Guild name validation starting")
    
    -- Guild name validation
    if #param < MIN_GUILD_NAME_LENGTH then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Guild name must be at least %d characters.", MIN_GUILD_NAME_LENGTH))
        return true
    end
    
    if #param > MAX_GUILD_NAME_LENGTH then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Guild name cannot exceed %d characters.", MAX_GUILD_NAME_LENGTH))
        return true
    end
    
    if not param:match("^[%w%s]+$") then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Guild name can only contain letters, numbers and spaces.")
        return true
    end
    
    print("DEBUG: Checking if player has guild")
    
    -- Check if player already has a guild
    if player:getGuild() then
        player:sendTextMessage(MESSAGE_INFO_DESCR, "You are already in a guild.")
        return true
    end
    
    print("DEBUG: Checking player level")
    
    -- Level requirement
    if player:getLevel() < MIN_LEVEL_CREATE then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("You need to be at least level %d to create a guild.", MIN_LEVEL_CREATE))
        return true
    end
    
    print("DEBUG: Checking money")
    
    -- Money requirement
    if not player:hasMoneyBank(CREATION_COST) then
        player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("You need %d gold in your bank to create a guild.", CREATION_COST))
        return true
    end
    
    print("DEBUG: Removing money and creating guild")
    
    -- Remove money and create guild
    player:removeMoneyBank(CREATION_COST)
    
    print("DEBUG: Calling Guild() function with name: " .. param)
    
    -- Create the guild using Guild() function
    local newGuild = Guild(param)
    print("DEBUG: Guild() returned: " .. tostring(newGuild))
    
    if newGuild then
        print("DEBUG: Guild created successfully, setting leader")
        -- Set player as guild leader
        newGuild:setLeader(player)
        newGuild:setMotd("Welcome to " .. param .. "!")
        
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("Guild '%s' has been created! You are now the leader.", param))
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        print("DEBUG: Guild creation completed successfully")
    else
        print("DEBUG: Guild creation failed, refunding money")
        -- If guild creation failed, refund the money
        player:addMoneyBank(CREATION_COST)
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Failed to create guild. Please try again.")
        return true
    end
    
    return true
end

createGuild:separator(" ")
createGuild:register()
