local playerManaChange = CreatureEvent("PlayerManaChange")

function playerManaChange.onManaChange(creature, attacker, primaryDamage, primaryType, secondaryDamage, secondaryType, origin)
    -- logger.info('playerManaChange.onManaChange')

    -- logger.info('creature:getName(): ' .. creature:getName())
    -- logger.info('attacker:getName(): ' .. attacker:getName())
    -- logger.info('primaryDamage: ' .. primaryDamage)
    -- logger.info('primaryType: ' .. primaryType)
    -- logger.info('secondaryDamage: ' .. secondaryDamage)
    -- logger.info('secondaryType: ' .. secondaryType)
    -- logger.info('origin: ' .. origin)

    return true
end

playerManaChange:register()