local npcName = "OpenCore"

local npcType = Game.createNpcType(npcName)
local npcConfig = {}

npcConfig.name = npcName
npcConfig.description = npcName

npcConfig.health = 100
npcConfig.maxHealth = npcConfig.health
npcConfig.walkInterval = 2000
npcConfig.walkRadius = 10

npcConfig.outfit = {
	lookType = 128,
	lookHead = 0,
	lookBody = 0,
	lookLegs = 0,
	lookFeet = 0,
	lookAddons = 2,
}

npcConfig.voices = {
	interval = 15000,
	chance = 20,
	{ text = "Welcome to the OpenCoreMMO Server!" },
}

-- npcConfig.flags = {
-- 	floorchange = false,
-- }

-- Npc shop
npcConfig.shop = {
	{ id = 2392, buy = 10000, sell = 100956 }, --fire sword
	{ id = 1989, buy = 6 }, --basket
	{ id = 2007, buy = 3 }, --bottle
	{ id = 7142, buy = 4 }, --bucket
	{ id = 2041, buy = 8 }, --candelabrum
	{ id = 2047, buy = 2 }, --candlestick
	{ id = 2578, buy = 280, sell = 75 }, --closed trap
	{ id = 2416, buy = 260, sell = 50 }, --crowbar
	{ id = 3941, buy = 2 }, --cup
	{ id = 1952, buy = 12 }, --document
	{ id = 2580, buy = 150, sell = 40 }, --fishing rod
	{ id = 1998, buy = 20 }, --green backpack
	{ id = 1991, buy = 4 }, --green bag
	{ id = 2420, buy = 35, sell = 6 }, --machete
	{ id = 1948, buy = 8 }, --parchment
	{ id = 2553, buy = 50, sell = 15 }, --pick
	{ id = 2035, buy = 6 }, --plate
	{ id = 2331, buy = 10 }, --present
	{ id = 6981, buy = 50, sell = 15 }, --rope
	{ id = 7724, buy = 5 }, --scroll
	{ id = 2550, buy = 50, sell = 10 }, --scythe
	{ id = 2554, buy = 50, sell = 8 }, --shovel
	{ id = 2050, buy = 2 }, --torch
	{ id = 2006, sell = 5 }, --vial
	{ id = 2036, buy = 20, sell = 6 }, --watch
	{ id = 2901, buy = 10, count = 1 }, --waterskin of water
	{ id = 2556, sell = 15 }, --wooden hammer
	{ id = 3976, buy = 1 }, --worm
}

-- Create keywordHandler and npcHandler
local keywordHandler = KeywordHandler:new()
local npcHandler = NpcHandler:new(keywordHandler)

-- onThink
npcType.onThink = function(npc, interval)
	--print('npcType.onThink')
	npcHandler:onThink(npc, interval)
end

-- onAppear
npcType.onAppear = function(npc, creature)
	print('npcType.onAppear')
	npcHandler:onAppear(npc, creature)
end

-- onDisappear
npcType.onDisappear = function(npc, creature)
	print('npcType.onDisappear')
	npcHandler:onDisappear(npc, creature)
end

-- onMove
npcType.onMove = function(npc, creature, fromPosition, toPosition)
	print('npcType.onMove')
	npcHandler:onMove(npc, creature, fromPosition, toPosition)
end

-- onSay
npcType.onSay = function(npc, creature, type, message)
	print('npcType.onSay')
	npcHandler:onSay(npc, creature, type, message)
end

-- onPlayerCloseChannel
npcType.onCloseChannel = function(npc, player)
	print('npcType.onCloseChannel')
	npcHandler:onCloseChannel(npc, player)
end

-- On buy npc shop message
npcType.onBuyItem = function(npc, player, id, subType, amount, ignore, inBackpacks, totalCost)
	print('npcType.onBuyItem')
	npc:sellItem(player, id, amount, subType, 0, ignore, inBackpacks)
end

-- On sell npc shop message
npcType.onSellItem = function(npc, player, id, subtype, amount, ignore, name, totalCost)
	print('npcType.onSellItem')
	player:sendTextMessage(MESSAGE_TRADE, string.format("Sold %ix %s for %i gold.", amount, name, totalCost))
end

-- On check npc shop message (look item)
npcType.onCheckItem = function(npc, player, id, subType) end

-- Function called by the callback "npcHandler:setCallback(CALLBACK_GREET, greetCallback)" in end of file
local function greetCallback(npc, player)
	npcHandler:setMessage(MESSAGE_GREET, "Hello |PLAYERNAME|, you need more info about {opencore}?")
	return true
end

-- On creature say callback
local function creatureSayCallback(npc, player, type, msg)
	local playerId = player:getId()
	if not npcHandler:checkInteraction(npc, player) then
		return false
	end

	if MsgContains(msg, "opencore") then
		if npcHandler:getTopic(playerId) == 0 then
			npcHandler:say({
				"The goal is for OpenCoreMMO to be an 'engine', that is, it will be \z
					a server with a 'clean' datapack, with as few things as possible, \z
					thus facilitating development and testing.",
				"See more on our {discord group}.",
			}, npc, player, 3000)
			npcHandler:setTopic(playerId, 1)
		end
	elseif MsgContains(msg, "discord group") then
		if npcHandler:getTopic(playerId) == 1 then
			npcHandler:say("This the our discord group link: {https://discordapp.com/invite/3NxYnyV}", npc, player)
			npcHandler:setTopic(playerId, 0)
		end
	end
	return true
end

-- Set to local function "greetCallback"
npcHandler:setCallback(CALLBACK_GREET, greetCallback)
-- Set to local function "creatureSayCallback"
npcHandler:setCallback(CALLBACK_MESSAGE_DEFAULT, creatureSayCallback)

-- Bye message
npcHandler:setMessage(MESSAGE_FAREWELL, "Yeah, good bye and don't come again!")
-- Walkaway message
npcHandler:setMessage(MESSAGE_WALKAWAY, "You not have education?")

npcHandler:addModule(FocusModule:new(), npcConfig.name, true, true, true)

-- Register npc
npcType:register(npcConfig)
