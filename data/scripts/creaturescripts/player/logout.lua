local playerLogout = CreatureEvent("PlayerLogout")

function playerLogout.onLogout(player)
	logger.info('playerLogout.onLogout')
	return true
end

playerLogout:register()
