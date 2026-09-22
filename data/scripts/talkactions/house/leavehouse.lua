---@type TalkAction
local talkAction = TalkAction("!leavehouse")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    local position = player:getPosition()
    local tile = player:getTile()
    local house = tile and tile:getHouse() or nil
    if not house then
        player:sendCancelMessage("You are not inside a house.")
        position:sendMagicEffect(CONST_ME_POFF)
        return true
    end

    if house:getOwnerGuid() ~= player:getGuid() then
        player:sendCancelMessage("You are not the owner of this house.")
        position:sendMagicEffect(CONST_ME_POFF)
        return true
    end

    house:setOwnerGuid(0)
    player:sendTextMessage(MESSAGE_INFO_DESCR, "You have successfully left your house.")
    position:sendMagicEffect(CONST_ME_POFF)
    return true
end

talkAction:register()
