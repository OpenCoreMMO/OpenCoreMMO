local talkAction = TalkAction("!test")

function talkAction.onSay(player, words, param)
    logger.info('executing talkAction from lua: ' .. words .. ' ' .. param)

    logger.info(player:getName())
    logger.info(player:getId())

	local loginStr = "TEST !!"
	player:sendTextMessage(MESSAGE_STATUS_DEFAULT, loginStr)
    
    Game.createMonster("Troll", player:getPosition(), true, true, player)

    -- local creature = Creature("GOD")

    -- if creature then
    --     logger.info(creature:getName())
    --     logger.info(creature:getId())

    --     if player == creature then
    --         logger.info('player == creature')
    --     end
    -- end

    -- local showInConsole = configManager.getBoolean(configKeys.SCRIPTS_CONSOLE_LOGS);

    -- logger.info(tostring(showInConsole))

    -- player:sendTextMessage(0, param, 0)

    -- local position = player:getPosition()

    -- logger.info(position:toString())

    -- position:sendMagicEffect(CONST_ME_POFF)

    -- Game.createItem(2159, 1, position)

    -- local direction = player:getDirection()

    -- -- Game.createMonster("Rat", position, true, true, player)

    -- logger.info('direction: ' .. direction)

    -- local nextPosition = position
    -- nextPosition:getNextPosition(direction)

    -- logger.info('nextPosition: ' .. nextPosition:toString())

    --  --Game.createMonster("Scarab", nextPosition)

    -- local storageValue = player:getStorageValue(1000)
    
    -- logger.info('storageValue: ' .. storageValue)

    -- if storageValue == -1 then
    --     player:setStorageValue(1000, 1996)
    --     storageValue = player:getStorageValue(1000)
    --     logger.info('storageValue2: ' .. storageValue)
    -- end

    local group = player:getGroup()
    
    logger.info('groupId: ' .. group:getId())
    logger.info('groupName: ' .. group:getName())
    logger.info('getAccess: ' .. tostring(group:getAccess()))

    logger.info('PlayerFlag_CanTalkRedChannel: ' .. tostring(group:hasFlag(PlayerFlag_CanTalkRedChannel)))
    logger.info('PlayerFlag_IgnoreYellCheck: ' .. tostring(group:hasFlag(PlayerFlag_IgnoreYellCheck)))
    logger.info('PlayerFlag_IgnoredByMonsters: ' .. tostring(group:hasFlag(PlayerFlag_IgnoredByMonsters)))
    logger.info('PlayerFlag_CannotBeAttacked: ' .. tostring(group:hasFlag(PlayerFlag_CannotBeAttacked)))

    addEvent(Game.broadcastMessage, 1000, "New record1: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    addEvent(Game.broadcastMessage, 2000, "New record2: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    addEvent(Game.broadcastMessage, 3000, "New record3: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    addEvent(Game.broadcastMessage, 4000, "New record4: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    
    local eventId = addEvent(Game.broadcastMessage, 5000, "New record5: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    
    logger.info('eventId5 = ' .. tostring(eventId))

    stopEvent(eventId);
    
    logger.info('end')

    return true
end

talkAction:separator(" ")
talkAction:register()
