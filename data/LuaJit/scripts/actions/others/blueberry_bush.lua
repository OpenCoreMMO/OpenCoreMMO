local blueBerryBush = Action()

function blueBerryBush.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    logger.info('blueBerryBush')
	item:transform(2786)
	item:decay()
	Game.createItem(2677, 3, fromPosition)
	-- player:addAchievementProgress("Bluebarian", 500)
	return true
end

blueBerryBush:id(2785)
blueBerryBush:register()