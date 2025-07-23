---@type Action
local knife = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function knife.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    return onUseKitchenKnife(player, item, fromPosition, target, toPosition, isHotkey)
end

knife:id(2566)
knife:register()
