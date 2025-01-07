local playerKill = CreatureEvent("PlayerKill")

function playerKill.onKill(player, target, lastHit)
	logger.info('playerKill.onKill')
	return true
end

playerKill:register()
