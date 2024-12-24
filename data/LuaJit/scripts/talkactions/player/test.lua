local talkAction = TalkAction("!test")

function talkAction.onSay(player, words, param)
	logger.info('executing talkAction from lua: '.. words .. ' ' .. param)

	logger.info(player)
	logger.info(player:getName())
	logger.info(player:getId())
	
	local creature = Creature("GOD")

	if creature then
		logger.info(creature)
		logger.info(creature:getName())
		logger.info(creature:getId())

		if player == creature then
			logger.info('player == creature')
		end
	end
	
	local showInConsole = configManager.getBoolean(configKeys.SCRIPTS_CONSOLE_LOGS);

	logger.info(showInConsole)

	player:sendTextMessage(0, param, 0);
	
	logger.info('end')

	return true
end

talkAction:separator(" ")
talkAction:register()
