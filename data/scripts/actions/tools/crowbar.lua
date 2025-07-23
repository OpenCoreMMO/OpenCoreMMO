---@type Action
local crowbar = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function crowbar.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseCrowbar(player, item, fromPosition, target, toPosition, isHotkey)
end

crowbar:id(2416)
crowbar:register()
