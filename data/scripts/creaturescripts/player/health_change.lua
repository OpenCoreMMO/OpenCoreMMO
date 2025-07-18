local playerHealthChange = CreatureEvent("PlayerHealthChange")

function playerHealthChange.onHealthChange(creature, attacker, primaryDamage, primaryType, secondaryDamage, secondaryType, origin)
    -- logger.info('playerHealthChange.onHealthChange')

    -- logger.info('creature:getName(): ' .. creature:getName())
    -- logger.info('attacker:getName(): ' .. attacker:getName())
    -- logger.info('primaryDamage: ' .. primaryDamage)
    -- logger.info('primaryType: ' .. primaryType)
    -- logger.info('secondaryDamage: ' .. secondaryDamage)
    -- logger.info('secondaryType: ' .. secondaryType)
    -- logger.info('origin: ' .. origin)

    return true
end

playerHealthChange:register()