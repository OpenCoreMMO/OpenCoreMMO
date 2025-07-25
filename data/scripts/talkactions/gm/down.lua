---@type TalkAction
local talkAction = TalkAction("/down")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    if not player:getGroup():getAccess() then
        return true
    end

    local position = player:getPosition()
    position.z = position.z + 1
    player:teleportTo(position)
    return true
end

talkAction:register()
