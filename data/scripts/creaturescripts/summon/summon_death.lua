local summonDeath = CreatureEvent("SummonDeath")

function summonDeath.onDeath(summon, corpse, killer, mostDamageKiller, lastHitUnjustified, mostDamageUnjustified)
    -- logger.info('summonDeath.onDeath')

    -- logger.info('summon:getName(): ' .. summon:getName())
    -- logger.info('killer:getName(): ' .. killer:getName())

    return true
end

summonDeath:register()