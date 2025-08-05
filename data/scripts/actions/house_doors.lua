-- Action script for house doors
local houseDoor = Action()

function houseDoor.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local house = Game.getHouseByPosition(toPosition) or Game.getHouseByPosition(fromPosition)
    
    if not house then
        return false -- Not a house door, use default behavior
    end
    
    local owner = house:getOwner()
    
    -- If house has no owner, show purchase option
    if owner == 0 then
        local modalWindow = ModalWindow(1000, "House Purchase", 
            string.format("Would you like to purchase %s?\n\nPrice: %d gold\nMonthly rent: %d gold\nSize: %d tiles", 
            house:getName(), house:getPrice(), house:getRent(), house:getSize()))
        
        modalWindow:addButton(1, "Buy House")
        modalWindow:addButton(2, "Cancel")
        modalWindow:setDefaultEnterButton(1)
        modalWindow:setDefaultEscapeButton(2)
        modalWindow:sendToPlayer(player)
        
        -- Store house ID for modal window callback
        player:setStorageValue(50000, house:getId())
        
        return true
    end
    
    -- Check if player can enter
    if not house:canEnter(player:getId()) then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, "The door seems to be sealed against unwanted intruders.")
        return true
    end
    
    -- Open/close door
    local doorId = item:getId()
    local newId = doorId + 1
    
    -- Simple door open/close logic (adjust based on your door IDs)
    if doorId % 2 == 0 then
        newId = doorId + 1  -- Open door
    else
        newId = doorId - 1  -- Close door
    end
    
    item:transform(newId)
    item:decay()
    
    return true
end

-- Modal window callback for house purchase
local housePurchaseModal = CreatureEvent("HousePurchaseModal")

function housePurchaseModal.onModalWindow(player, modalWindowId, buttonId, choiceId)
    if modalWindowId ~= 1000 then
        return false
    end
    
    if buttonId ~= 1 then -- Not the "Buy House" button
        return true
    end
    
    local houseId = player:getStorageValue(50000)
    if houseId == -1 then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Error: No house selected.")
        return true
    end
    
    player:setStorageValue(50000, -1) -- Clear storage
    
    local success = Game.purchaseHouse(houseId, player:getId())
    if success then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "Congratulations! You have successfully purchased the house.")
    else
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to purchase house. You might not have enough money or the house is no longer available.")
    end
    
    return true
end

housePurchaseModal:register()

-- Register for door IDs (adjust based on your game data)
-- Example door IDs - you would need to configure these based on your items.json
local doorIds = {
    1209, 1210, 1211, 1212, 1213, 1214, 1215, 1216, -- Wooden doors
    1217, 1218, 1219, 1220, 1221, 1222, 1223, 1224, -- Stone doors
    1225, 1226, 1227, 1228, 1229, 1230, 1231, 1232, -- Metal doors
    3535, 3536, 3537, 3538, 3539, 3540, 3541, 3542  -- Special doors
}

for _, doorId in ipairs(doorIds) do
    houseDoor:id(doorId)
end

houseDoor:register()

-- House management door action (for house owners)
local houseManagementDoor = Action()

function houseManagementDoor.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local house = Game.getHouseByPosition(toPosition) or Game.getHouseByPosition(fromPosition)
    
    if not house then
        return false
    end
    
    local owner = house:getOwner()
    
    -- Only owner can access management options
    if owner ~= player:getId() then
        return houseDoor.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    end
    
    -- Right-click on door shows management options
    local modalWindow = ModalWindow(1001, "House Management", 
        string.format("Managing: %s\nRent: %d gold/month\nStatus: %s", 
        house:getName(), house:getRent(), house:isPaid() and "Paid" or "Overdue"))
    
    modalWindow:addButton(1, "Guest List")
    modalWindow:addButton(2, "Pay Rent")
    modalWindow:addButton(3, "Transfer House")
    modalWindow:addButton(4, "Leave House")
    modalWindow:addButton(5, "Cancel")
    modalWindow:setDefaultEscapeButton(5)
    modalWindow:sendToPlayer(player)
    
    -- Store house ID for modal window callback
    player:setStorageValue(50001, house:getId())
    
    return true
end

-- Modal window callback for house management
local houseManagementModal = CreatureEvent("HouseManagementModal")

function houseManagementModal.onModalWindow(player, modalWindowId, buttonId, choiceId)
    if modalWindowId ~= 1001 then
        return false
    end
    
    local houseId = player:getStorageValue(50001)
    if houseId == -1 then
        return true
    end
    
    local house = Game.getHouseById(houseId)
    if not house or house:getOwner() ~= player:getId() then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Error: You don't own this house.")
        return true
    end
    
    if buttonId == 1 then -- Guest List
        local guests = house:getGuests()
        if #guests == 0 then
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "No guests in your house.")
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "House guests:")
            for _, guestId in ipairs(guests) do
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, string.format("- Player ID: %d", guestId))
            end
        end
        
    elseif buttonId == 2 then -- Pay Rent
        local rent = house:getRent()
        local money = player:getMoney()
        
        if money >= rent then
            player:removeMoney(rent)
            local success = house:payRent(player:getId())
            if success then
                player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You paid %d gold in rent.", rent))
            else
                player:addMoney(rent) -- Refund if payment failed
                player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to pay rent.")
            end
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, string.format("You need %d gold to pay rent.", rent))
        end
        
    elseif buttonId == 3 then -- Transfer House
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Type the name of the player you want to transfer this house to:")
        -- This would require additional text input handling
        
    elseif buttonId == 4 then -- Leave House
        local success = house:transferOwnership(player:getId(), 0)
        if success then
            player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "You have left the house. It is now available for purchase.")
        else
            player:sendTextMessage(MESSAGE_STATUS_CONSOLE_RED, "Failed to leave house.")
        end
    end
    
    player:setStorageValue(50001, -1) -- Clear storage
    return true
end

houseManagementModal:register()

-- Register management action for specific items (like house signs)
-- You would configure specific item IDs that trigger house management
local managementItems = {1234, 1235, 1236} -- Example item IDs for house signs

for _, itemId in ipairs(managementItems) do
    houseManagementDoor:id(itemId)
end

houseManagementDoor:register()

print("House door and management scripts loaded successfully!")
