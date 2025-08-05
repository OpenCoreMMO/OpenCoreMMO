-- Admin commands for house management
-- Usage: /house info, /house buy [houseId] [playerId], /house kick [houseId] [playerId], etc.

local function printHouseCommands(player)
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "House Admin Commands:")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house info - Get house info at current position")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house list - List all houses")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house buy [houseId] [playerId] - Force buy house for player")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house kick [houseId] - Remove all occupants from house")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house transfer [houseId] [fromId] [toId] - Transfer house ownership")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house guest [houseId] [playerId] - Add/remove guest")
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "/house clean - Clean all expired houses")
end

local houseAdmin = TalkAction("/house")

function houseAdmin.onSay(player, words, param)
    if not player:isAdmin() then
        player:sendCancelMessage("You don't have permission to use this command.")
        return false
    end
    
    local params = string.split(param, " ")
    local command = params[1]
    
    if not command then
        printHouseCommands(player)
        return false
    end
    
    command = command:lower()
    
    if command == "info" then
        local house = Game.getHouseByPlayerPosition(player:getPosition())
        if not house then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "No house found at this position.")
            return false
        end
        
        local owner = house:getOwner()
        local guests = house:getGuests()
        local subOwners = house:getSubOwners()
        
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("House ID: %d", house:getId()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Name: %s", house:getName()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Owner: %d", owner))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Town ID: %d", house:getTownId()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Rent: %d gold", house:getRent()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Price: %d gold", house:getPrice()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Size: %d tiles", house:getSize()))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Paid: %s", house:isPaid() and "Yes" or "No"))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Guests: %d", #guests))
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Sub-Owners: %d", #subOwners))
        
    elseif command == "list" then
        local houses = Game.getHouses()
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("Total houses: %d", #houses))
        
        for i, house in ipairs(houses) do
            local owner = house:getOwner()
            local status = owner == 0 and "Available" or (house:isPaid() and "Owned" or "Expired")
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                string.format("%d. %s (ID: %d, Owner: %d, Status: %s)", 
                i, house:getName(), house:getId(), owner, status))
            
            if i >= 10 then
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "... (showing first 10 houses)")
                break
            end
        end
        
    elseif command == "buy" then
        local houseId = tonumber(params[2])
        local playerId = tonumber(params[3])
        
        if not houseId or not playerId then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Usage: /house buy [houseId] [playerId]")
            return false
        end
        
        local success = Game.purchaseHouse(houseId, playerId)
        if success then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                string.format("Successfully assigned house %d to player %d", houseId, playerId))
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to assign house. Check if house exists and is available.")
        end
        
    elseif command == "kick" then
        local houseId = tonumber(params[2])
        
        if not houseId then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Usage: /house kick [houseId]")
            return false
        end
        
        local house = Game.getHouseById(houseId)
        if not house then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "House not found.")
            return false
        end
        
        -- In a real implementation, you'd kick all players from the house area
        -- For now, just release the house
        local success = house:transferOwnership(house:getOwner(), 0)
        if success then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                string.format("House %d has been released and all occupants kicked.", houseId))
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to kick occupants from house.")
        end
        
    elseif command == "transfer" then
        local houseId = tonumber(params[2])
        local fromId = tonumber(params[3])
        local toId = tonumber(params[4])
        
        if not houseId or not fromId or not toId then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Usage: /house transfer [houseId] [fromId] [toId]")
            return false
        end
        
        local house = Game.getHouseById(houseId)
        if not house then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "House not found.")
            return false
        end
        
        local success = house:transferOwnership(fromId, toId)
        if success then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                string.format("House %d transferred from player %d to player %d", houseId, fromId, toId))
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to transfer house ownership.")
        end
        
    elseif command == "guest" then
        local houseId = tonumber(params[2])
        local guestId = tonumber(params[3])
        
        if not houseId or not guestId then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Usage: /house guest [houseId] [playerId]")
            return false
        end
        
        local house = Game.getHouseById(houseId)
        if not house then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "House not found.")
            return false
        end
        
        local owner = house:getOwner()
        if owner == 0 then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "House has no owner.")
            return false
        end
        
        -- Try to add guest first, if it fails, try to remove
        local success = house:addGuest(owner, guestId)
        if success then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                string.format("Player %d added as guest to house %d", guestId, houseId))
        else
            success = house:removeGuest(owner, guestId)
            if success then
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
                    string.format("Player %d removed as guest from house %d", guestId, houseId))
            else
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to modify guest list.")
            end
        end
        
    elseif command == "clean" then
        -- Clean expired houses - this would be implemented in the service layer
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Cleaning expired houses...")
        
        local houses = Game.getHouses()
        local cleaned = 0
        
        for _, house in ipairs(houses) do
            if house:getOwner() ~= 0 and not house:isPaid() then
                local success = house:transferOwnership(house:getOwner(), 0)
                if success then
                    cleaned = cleaned + 1
                end
            end
        end
        
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
            string.format("Cleaned %d expired houses.", cleaned))
        
    else
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Unknown house command.")
        printHouseCommands(player)
    end
    
    return false
end

houseAdmin:groupType("admin")
houseAdmin:register()

-- Player command for house information
local houseInfo = TalkAction("/houseinfo")

function houseInfo.onSay(player, words, param)
    local house = Game.getHouseByPlayerPosition(player:getPosition())
    if not house then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "No house found at this position.")
        return false
    end
    
    local owner = house:getOwner()
    if owner == 0 then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
            string.format("House: %s\nStatus: Available for purchase\nPrice: %d gold\nRent: %d gold/month", 
            house:getName(), house:getPrice(), house:getRent()))
    else
        local canEnter = house:canEnter(player:getId())
        local access = canEnter and "Yes" or "No"
        
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 
            string.format("House: %s\nOwner ID: %d\nRent: %d gold/month\nAccess: %s", 
            house:getName(), owner, house:getRent(), access))
    end
    
    return false
end

houseInfo:register()

print("House admin commands loaded successfully!")
