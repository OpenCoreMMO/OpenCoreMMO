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
    print("DEBUG: !createguild command called by " .. player:getName())
    print("DEBUG: param = '" .. tostring(param) .. "'")
    
    if not param or param == "" then
        print("DEBUG: No param provided")
        player:sendTextMessage(MESSAGE_INFO_DESCR, "Usage: !createguild <guild name>")
        return true
    end
    
    -- Simplified validation
    if #param < MIN_GUILD_NAME_LENGTH then
        print("DEBUG: Name too short")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild name must be at least 4 characters long.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    if #param > MAX_GUILD_NAME_LENGTH then
        print("DEBUG: Name too long")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild name cannot exceed 29 characters.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Check if name contains only valid characters (letters, numbers, spaces)
    if not param:match("^[%a%d%s]+$") then
        print("DEBUG: Invalid characters in guild name")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild name can only contain letters, numbers, and spaces.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Check if name starts or ends with space
    if param:match("^%s") or param:match("%s$") then
        print("DEBUG: Guild name starts or ends with space")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild name cannot start or end with spaces.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Check for consecutive spaces
    if param:match("%s%s") then
        print("DEBUG: Guild name has consecutive spaces")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild name cannot contain consecutive spaces.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Check if guild with this name already exists BEFORE removing money
    if GuildExists(param) then
        print("DEBUG: Guild name already exists")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "A guild with the name '" .. param .. "' already exists. Please choose a different name.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Check if player already has a guild
    local currentGuild = player:getGuild()
    if currentGuild then
        print("DEBUG: Player already in guild")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You are already in a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Level requirement
    local playerLevel = player:getLevel()
    print("DEBUG: Player level = " .. playerLevel)
    if playerLevel < MIN_LEVEL_CREATE then
        print("DEBUG: Level too low")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You need to be at least level 20 to create a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Money requirement
    local hasMoneyBank = player:hasMoneyBank(CREATION_COST)
    print("DEBUG: Has money bank = " .. tostring(hasMoneyBank))
    if not hasMoneyBank then
        print("DEBUG: Not enough money")
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You need 10,000 gold in your bank to create a guild.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return true
    end
    
    -- Only remove money after all validations pass
    print("DEBUG: All validations passed, removing money from bank")
    local moneyRemoved = player:removeMoneyBank(CREATION_COST)
    print("DEBUG: Money removed = " .. tostring(moneyRemoved))
    
    print("DEBUG: Creating guild with name: '" .. param .. "'")
    
    -- Create the guild using Guild() function
    local newGuild = Guild(param)
    print("DEBUG: Guild() returned: " .. tostring(newGuild))
    
    if newGuild then
        print("DEBUG: Guild created successfully")
        print("DEBUG: Guild name: " .. newGuild:getName())
        print("DEBUG: Guild ID: " .. newGuild:getId())
        
        -- Set MOTD first
        newGuild:setMotd("Welcome to " .. param .. "!")
        print("DEBUG: MOTD set")
        
        -- Set player as guild leader
        newGuild:setLeader(player)
        print("DEBUG: Leader set successfully")
        
        -- Join player to the guild (this will trigger network updates)
        player:setGuild(newGuild)
        print("DEBUG: Player joined guild")
        
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Guild '" .. param .. "' has been created! You are now the leader.")
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
        print("DEBUG: Success message sent")
    else
        print("DEBUG: Guild creation failed unexpectedly")
        
        -- If Guild() fails after validation, refund the money
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Failed to create guild. An unexpected error occurred.")
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        
        -- Refund the money since guild creation failed
        print("DEBUG: Refunding money due to unexpected guild creation failure")
        player:addMoneyBank(CREATION_COST)
        print("DEBUG: Money refunded")
    end
    
    return true
end

createGuild:separator(" ")
createGuild:register()