local playerDeath = CreatureEvent("PlayerDeath")

function playerDeath.onDeath(player, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
	logger.info('playerDeath.onDeath')
	return true
end

playerDeath:register()
