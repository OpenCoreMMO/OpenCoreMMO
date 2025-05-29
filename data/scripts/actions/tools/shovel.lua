local shovel = Action()

function shovel.onUse(player, item, fromPosition, target, toPosition, isHotkey)
	return onUseShovel(player, item, fromPosition, target, toPosition, isHotkey)
end

shovel:id(2554, 5710)
shovel:register()
