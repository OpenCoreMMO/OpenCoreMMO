---@type Action
local blueBerryBush = Action()

---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function blueBerryBush.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    item:transform(2786)
    item:decay()
    Game.createItem(2677, 3, fromPosition)
    return true
end

blueBerryBush:id(2785)
blueBerryBush:register()