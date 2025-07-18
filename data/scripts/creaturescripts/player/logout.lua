local playerLogout = CreatureEvent("PlayerLogout")

function playerLogout.onLogout(player)
    logger.info('playerLogout.onLogout')

    -- Events
    player:unregisterEvent("ExtendedOpcode")
    player:unregisterEvent("PlayerDeath")
    player:unregisterEvent("PlayerAdvance")
    player:unregisterEvent("PlayerPrepareDeath")
    player:unregisterEvent("PlayerTextEdit")
    player:unregisterEvent("PlayerKill")
    player:unregisterEvent("PlayerManaChange")
    player:unregisterEvent("PlayerHealthChange")

    return true
end

playerLogout:register()
