local knife = Action()

function knife.onUse(player, item, fromPosition, target, toPosition, isHotkey)
	return onUseKitchenKnife(player, item, fromPosition, target, toPosition, isHotkey)
end

knife:id(2566)
knife:register()
