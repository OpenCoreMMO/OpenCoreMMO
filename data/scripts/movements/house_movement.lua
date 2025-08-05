-- Movement script for house tiles
local houseMovement = MoveEvent()

function houseMovement.onStepIn(creature, item, position, fromPosition)
    if not creature:isPlayer() then
        return true
    end
    
    local player = creature
    local house = Game.getHouseByPosition(position)
    
    if not house then
        return true
    end
    
    local owner = house:getOwner()
    
    -- If house has no owner, show purchase option
    if owner == 0 then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 
            string.format("Welcome to %s! This house is available for purchase. Price: %d gold, Rent: %d gold/month. Say 'buy house' to purchase.", 
            house:getName(), house:getPrice(), house:getRent()))
        return true
    end
    
    -- Check if player can enter
    if not house:canEnter(player:getId()) then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, "You are not invited.")
        player:teleportTo(fromPosition)
        player:getPosition():sendMagicEffect(CONST_ME_TELEPORT)
        return false
    end
    
    -- Welcome message for authorized players
    if house:getOwner() == player:getId() then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 
            string.format("Welcome home to %s! Rent: %d gold/month. %s", 
            house:getName(), house:getRent(), house:isPaid() and "Rent is paid." or "WARNING: Rent is overdue!"))
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 
            string.format("Welcome to %s. You are here as a guest.", house:getName()))
    end
    
    return true
end

function houseMovement.onStepOut(creature, item, position, fromPosition)
    if not creature:isPlayer() then
        return true
    end
    
    local player = creature
    local house = Game.getHouseByPosition(fromPosition)
    
    if house then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, 
            string.format("You have left %s.", house:getName()))
    end
    
    return true
end

-- Register for house tiles (you would need to configure this based on your tile IDs)
-- This is an example - adjust the tile IDs based on your game data
for tileId = 1350, 1370 do  -- Example range of house floor tile IDs
    houseMovement:id(tileId)
end

houseMovement:register()

print("House movement script loaded successfully!")
