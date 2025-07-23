---@type Action
local machete = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function machete.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseMachete(player, item, fromPosition, target, toPosition, isHotkey)
end

machete:id(2420, 2442)
machete:register()
