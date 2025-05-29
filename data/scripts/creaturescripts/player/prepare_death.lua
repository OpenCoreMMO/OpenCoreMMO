local playerPrepareDeath = CreatureEvent("PlayerPrepareDeath")

function playerPrepareDeath.onPrepareDeath(player, killer, realDamage)
	logger.info('playerPrepareDeath.onPrepareDeath')
	return true
end

playerPrepareDeath:register()
