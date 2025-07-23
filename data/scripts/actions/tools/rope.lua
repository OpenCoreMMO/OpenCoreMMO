---@type Action
local rope = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function rope.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseRope(player, item, fromPosition, target, toPosition, isHotkey)
end

rope:id(2120, 7731)
rope:register()
