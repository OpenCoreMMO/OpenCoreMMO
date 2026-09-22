---@type TalkAction
local talkAction = TalkAction("!sellhouse")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    local tradePartner = Player(param)
    if not tradePartner or tradePartner == player then
        player:sendCancelMessage("Trade player not found.")
        return true
    end

    local tile = player:getTile()
    local house = tile and tile:getHouse() or nil
    if not house then
        player:sendCancelMessage("You must stand in your house to initiate the trade.")
        return true
    end

    local returnValue = house:startTrade(player, tradePartner)
    if returnValue ~= RETURNVALUE_NOERROR then
        player:sendCancelMessage(returnValue)
    end
    return true
end

talkAction:separator(" ")
talkAction:register()
