local talkAction = TalkAction("!test")

function talkAction.onSay(player, words, param)
	logger.info('executing talkAction from lua: '.. words .. ' ' .. param)

	logger.info(player:getName())
	logger.info(player:getId())
	
	local creature = Creature("GOD")

	if creature then
		logger.info(creature:getName())
		logger.info(creature:getId())

		if player == creature then
			logger.info('player == creature')
		end
	end
	
	local showInConsole = configManager.getBoolean(configKeys.SCRIPTS_CONSOLE_LOGS);

	logger.info(tostring(showInConsole))

	player:sendTextMessage(0, param, 0)
	
	local position = player:getPosition()

	logger.info(position:toString())

	position:sendMagicEffect(CONST_ME_POFF)

	Game.createItem(2159, 1, position)

	local direction = player:getDirection()
	
	Game.createMonster("Rat", position, true, true, player)

	logger.info('direction: ' .. direction)

	local nextPosition = position
	nextPosition:getNextPosition(direction)
	
	logger.info('nextPosition: ' .. nextPosition:toString())

	Game.createMonster("Scarab", nextPosition)

	logger.info('end')

	return true
end

talkAction:separator(" ")
talkAction:register()
