-- House management actions for players
-- This script handles various house-related actions

local houseActions = {
    -- Action IDs for house doors and items
    HOUSE_DOOR_ID = 1234,
    HOUSE_SIGN_ID = 1235,
    HOUSE_CHEST_ID = 1236,
}

-- Handle house door usage
function onUse_HouseDoor(player, item, fromPosition, target, toPosition, isHotkey)
    local house = Game.getHouseByPosition(toPosition)
    if not house then
        return false
    end
    
    local playerId = player:getId()
    
    -- Check if player can enter the house
    if not house:canEnter(playerId) then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, "You are not invited to this house.")
        return false
    end
    
    -- Teleport player to/from house
    local playerPos = player:getPosition()
    local houseEntry = house:getEntry()
    
    -- Simple teleportation logic - in real implementation you'd want more sophisticated entry/exit handling
    if playerPos == houseEntry then
        -- Player is at house entry, teleport inside (simplified)
        local insidePos = Position(houseEntry.x + 1, houseEntry.y, houseEntry.z)
        player:teleportTo(insidePos)
    else
        -- Player is inside, teleport to entry
        player:teleportTo(houseEntry)
    end
    
    return true
end

-- Handle house sign usage (information and management)
function onUse_HouseSign(player, item, fromPosition, target, toPosition, isHotkey)
    local house = Game.getHouseByPosition(toPosition)
    if not house then
        return false
    end
    
    local playerId = player:getId()
    local owner = house:getOwner()
    
    if owner == 0 then
        -- House is available for purchase
        local price = house:getPrice()
        local rent = house:getRent()
        
        local text = string.format("House for Sale\n\nHouse: %s\nPrice: %d gold coins\nRent: %d gold coins per month\n\nWould you like to buy this house?", 
            house:getName(), price, rent)
        
        local window = ModalWindow(1000, "House for Sale", text)
        window:addButton(1, "Buy House")
        window:addButton(2, "Cancel")
        window:addChoice(1, string.format("Yes, buy for %d gold", price))
        window:setDefaultEscapeButton(2)
        window:setDefaultEnterButton(1)
        
        window:sendToPlayer(player)
        return true
    else
        -- House is owned
        if house:canEdit(playerId) then
            -- Owner or sub-owner can manage the house
            showHouseManagement(player, house)
        else
            -- Show house information only
            showHouseInfo(player, house)
        end
        return true
    end
end

-- Show house management window for owners/sub-owners
function showHouseManagement(player, house)
    local text = string.format("House Management\n\nHouse: %s\nOwner ID: %d\nPaid until: %s\nRent: %d gold/month", 
        house:getName(), 
        house:getOwner(),
        house:isPaid() and "Paid" or "EXPIRED",
        house:getRent())
    
    local window = ModalWindow(1001, "House Management", text)
    window:addButton(1, "Manage Guests")
    window:addButton(2, "Manage Sub-Owners") 
    window:addButton(3, "Pay Rent")
    window:addButton(4, "Transfer House")
    window:addButton(5, "Leave House")
    window:addButton(6, "Close")
    window:setDefaultEscapeButton(6)
    
    window:sendToPlayer(player)
end

-- Show house information for non-owners
function showHouseInfo(player, house)
    local ownerName = "Unknown" -- You'd want to look up the actual player name
    local text = string.format("House Information\n\nHouse: %s\nOwner: %s\nRent: %d gold/month", 
        house:getName(), ownerName, house:getRent())
    
    local window = ModalWindow(1002, "House Information", text)
    window:addButton(1, "Close")
    window:setDefaultEscapeButton(1)
    window:setDefaultEnterButton(1)
    
    window:sendToPlayer(player)
end

-- Handle modal window selections
function onModalWindow(player, modalWindowId, buttonId, choiceId)
    if modalWindowId == 1000 then -- House purchase
        if buttonId == 1 and choiceId == 1 then -- Buy house
            local house = Game.getHouseByPlayerPosition(player:getPosition())
            if house and house:getOwner() == 0 then
                local success = Game.purchaseHouse(house:getId(), player:getId())
                if success then
                    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Congratulations! You have successfully bought the house.")
                else
                    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to purchase the house. You may not have enough money.")
                end
            end
        end
        return true
    elseif modalWindowId == 1001 then -- House management
        local house = Game.getHouseByPlayerPosition(player:getPosition())
        if not house or not house:canEdit(player:getId()) then
            return true
        end
        
        if buttonId == 1 then -- Manage Guests
            showGuestManagement(player, house)
        elseif buttonId == 2 then -- Manage Sub-Owners
            showSubOwnerManagement(player, house)
        elseif buttonId == 3 then -- Pay Rent
            -- Implement rent payment logic
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Rent payment feature will be implemented here.")
        elseif buttonId == 4 then -- Transfer House
            -- Implement house transfer logic
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "House transfer feature will be implemented here.")
        elseif buttonId == 5 then -- Leave House
            local success = house:transferOwnership(player:getId(), 0) -- Release house
            if success then
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "You have left the house.")
            end
        end
        return true
    end
    
    return false
end

-- Show guest management interface
function showGuestManagement(player, house)
    local guests = house:getGuests()
    local guestList = ""
    
    for i, guestId in ipairs(guests) do
        guestList = guestList .. string.format("Guest %d: ID %d\n", i, guestId)
    end
    
    if guestList == "" then
        guestList = "No guests invited."
    end
    
    local text = string.format("Guest Management\n\n%s\n\nEnter guest ID to add/remove:", guestList)
    
    -- In a real implementation, you'd use a text input dialog
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Guest management interface would be shown here.")
end

-- Show sub-owner management interface  
function showSubOwnerManagement(player, house)
    local subOwners = house:getSubOwners()
    local subOwnerList = ""
    
    for i, subOwnerId in ipairs(subOwners) do
        subOwnerList = subOwnerList .. string.format("Sub-Owner %d: ID %d\n", i, subOwnerId)
    end
    
    if subOwnerList == "" then
        subOwnerList = "No sub-owners assigned."
    end
    
    local text = string.format("Sub-Owner Management\n\n%s\n\nEnter player ID to add/remove:", subOwnerList)
    
    -- In a real implementation, you'd use a text input dialog
    player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Sub-owner management interface would be shown here.")
end

-- Register action IDs
local actions = Action()
actions:id(houseActions.HOUSE_DOOR_ID)
actions:onUse(onUse_HouseDoor)
actions:register()

local actions2 = Action()
actions2:id(houseActions.HOUSE_SIGN_ID)
actions2:onUse(onUse_HouseSign)
actions2:register()

print("House management actions loaded successfully!")
