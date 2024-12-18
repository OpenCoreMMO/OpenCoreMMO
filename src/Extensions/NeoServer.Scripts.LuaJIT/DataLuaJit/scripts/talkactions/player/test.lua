local talkAction = TalkAction("!test")

function talkAction.onSay(player, words, param)
	print('executing talkAction from lua: '.. words .. ' ' .. param)

	print(player)
	print(player:getName())
	print(player:getId())
	
	local creature = Creature("GOD")

	if creature then
		print(creature)
		print(creature:getName())
		print(creature:getId())

		if player == creature then
			print('player == creature')
		end
	end
	
	local showInConsole = configManager.getBoolean(configKeys.SCRIPTS_CONSOLE_LOGS);

	print(showInConsole)

	player:sendTextMessage(0, param, 0);
	
	print('end')

	return true
end

talkAction:separator(" ")
talkAction:register()
