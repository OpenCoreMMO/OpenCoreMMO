-- local talkAction = TalkAction("!help")

-- function Player.teste(self, message)
-- 	print('message from player:teste() ' .. self:getName() .. ' - ' .. message)
-- end

-- local function testEvent1()
-- 	print('testEvent1')
-- end

-- local function testEvent2(param)
-- 	print('testEvent2 param: ' .. param)
-- end

-- function talkAction.onSay(player, words, param)
-- 	print('executing talkAction from lua: '.. words .. ' ' .. param)
	
-- 	player:teste('aaaaaaaaaaaaaa')
-- 	print(player:getName())
-- 	print(player:getId())

-- 	local creature = Creature("Muniz")
-- 	print(creature:getName())
-- 	print(creature:getId())

-- 	if player == creature then
-- 		print('player == creature')
-- 	end

-- 	local showScriptsLogInConsole = configManager.getBoolean(configKeys.SCRIPTS_CONSOLE_LOGS);

-- 	print(showScriptsLogInConsole)

-- 	local evt1Id = addEvent(testEvent1, 1000)
-- 	print('evt1Id: ' .. evt1Id)

-- 	local evt2Id = addEvent(testEvent1, 8000)
-- 	print('evt2Id: ' .. evt2Id)
-- 	stopEvent(evt2Id)

-- 	local evt3Id = addEvent(testEvent2, 10000, player:getName())
-- 	print('evt3Id: ' .. evt3Id)

-- 	return true
-- end

-- talkAction:separator(" ")
-- talkAction:register()
