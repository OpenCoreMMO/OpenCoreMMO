---@type Action
local scythe = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function scythe.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseScythe(player, item, fromPosition, target, toPosition, isHotkey)
end

scythe:id(2550)
scythe:register()
