---@type Action
local pick = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function pick.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUsePick(player, item, fromPosition, target, toPosition, isHotkey)
end

pick:id(2553)
pick:register()
