local onthinkExample = CreatureEvent("onthinkExample")

function onthinkExample.onThink(player, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
	logger.info('onthinkExample.onThink2')
	return true
end

onthinkExample:register()
