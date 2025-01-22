local playerAdvance = CreatureEvent("PlayerAdvance")

function playerAdvance.onAdvance(player, skill, killer, oldValue, newValue)
	logger.info('playerAdvance.onAdvance')
	return true
end

playerAdvance:register()
