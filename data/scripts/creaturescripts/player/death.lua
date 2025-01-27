local playerDeath = CreatureEvent("PlayerDeath")

function playerDeath.onDeath(player, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
	logger.info('playerDeath.onDeath')

	local message = string.format("%s was KILLED by %s", player:getName(), killer:getName())
    sendChannelMessage(9, TALKTYPE_CHANNEL_R1, message)

	return true
end

playerDeath:register()
