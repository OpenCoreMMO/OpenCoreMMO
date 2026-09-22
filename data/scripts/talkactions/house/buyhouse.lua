---@type TalkAction
local talkAction = TalkAction("!buyhouse")

local config = {level = 1}

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    local housePrice = configManager.getNumber(configKeys.HOUSE_PRICE)
    if housePrice == -1 then
        return false
    end

    if player:getLevel() < config.level then
        player:sendCancelMessage("You need level " .. config.level ..
                                     " or higher to buy a house.")
        return true
    end

    if not player:canOwnHouse() then
        player:sendCancelMessage("You need a premium account.")
        return true
    end

    -- OpenCoreMMO getNextPosition returns a new position (it does not mutate in place).
    local position = player:getPosition():getNextPosition(player:getDirection())

    local tile = Tile(position)
    local house = tile and tile:getHouse()
    if not house then
        player:sendCancelMessage(
            "You have to be looking at the door of the house you would like to buy.")
        return true
    end

    if house:getOwnerGuid() > 0 then
        player:sendCancelMessage("This house already has an owner.")
        return true
    end

    if player:getHouse() then
        player:sendCancelMessage("You are already the owner of a house.")
        return true
    end

    local price = house:getTileCount() * housePrice
    if not player:removeTotalMoney(price) then
        player:sendCancelMessage("You do not have enough money.")
        return true
    end

    house:setOwnerGuid(player:getGuid())
    player:sendTextMessage(MESSAGE_INFO_DESCR,
                           "You have successfully bought this house, be sure to have the money for the rent in the bank.")
    return true
end

talkAction:register()
