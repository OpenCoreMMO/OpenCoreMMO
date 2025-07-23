---@type Action
local shovel = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function shovel.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseShovel(player, item, fromPosition, target, toPosition, isHotkey)
end

shovel:id(2554, 5710)
shovel:register()
