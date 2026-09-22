---@type TalkAction
local talkAction = TalkAction("/owner")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    if not player:getGroup():getAccess() then
        return true
    end

    local tile = player:getTile()
    local house = tile and tile:getHouse() or nil
    if not house then
        player:sendCancelMessage("You are not inside a house.")
        return true
    end

    if param == "" or param == "none" then
        house:setOwnerGuid(0)
        return true
    end

    local targetPlayer = Player(param)
    if not targetPlayer then
        player:sendCancelMessage("Player not found.")
        return true
    end

    house:setOwnerGuid(targetPlayer:getGuid())
    return true
end

talkAction:separator(" ")
talkAction:register()
