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
    { itemName = "fire sword", id = 2392, buy = 10000, sell = 100956 },
    { itemName = "basket", id = 1989, buy = 6 },
    { itemName = "bottle", id = 2007, buy = 3 },
    { itemName = "bucket", id = 7142, buy = 4 },
    { itemName = "candelabrum", id = 2041, buy = 8 },
    { itemName = "candlestick", id = 2047, buy = 2 },
    { itemName = "closed trap", id = 2578, buy = 280, sell = 75 },
    { itemName = "crowbar", id = 2416, buy = 260, sell = 50 },
    { itemName = "cup", id = 3941, buy = 2 },
    { itemName = "document", id = 1952, buy = 12 },
    { itemName = "fishing rod", id = 2580, buy = 150, sell = 40 },
    { itemName = "green backpack", id = 1998, buy = 20 },
    { itemName = "green bag", id = 1991, buy = 4 },
    { itemName = "machete", id = 2420, buy = 35, sell = 6 },
    { itemName = "parchment", id = 1948, buy = 8 },
    { itemName = "pick", id = 2553, buy = 50, sell = 15 },
    { itemName = "plate", id = 2035, buy = 6 },
    { itemName = "present", id = 2331, buy = 10 },
    { itemName = "rope", id = 6981, buy = 50, sell = 15 },
    { itemName = "scroll", id = 7724, buy = 5 },
    { itemName = "scythe", id = 2550, buy = 50, sell = 10 },
    { itemName = "shovel", id = 2554, buy = 50, sell = 8 },
    { itemName = "torch", id = 2050, buy = 2 },
    { itemName = "vial", id = 2006, sell = 5 },
    { itemName = "watch", id = 2036, buy = 20, sell = 6 },
    { itemName = "waterskin of water", id = 2901, buy = 10, count = 1 },
    { itemName = "wooden hammer", id = 2556, sell = 15 },
    { itemName = "worm", id = 3976, buy = 1 },
}

-- Create keywordHandler and npcHandler
local keywordHandler = KeywordHandler:new()
local npcHandler = NpcHandler:new(keywordHandler)

-- onThink
npcType.onThink = function(npc, interval)
    npcHandler:onThink(npc, interval)
end

-- onAppear
npcType.onAppear = function(npc, creature)
    npcHandler:onAppear(npc, creature)
end

-- onDisappear
npcType.onDisappear = function(npc, creature)
    npcHandler:onDisappear(npc, creature)
end

-- onMove
npcType.onMove = function(npc, creature, fromPosition, toPosition)
    npcHandler:onMove(npc, creature, fromPosition, toPosition)
end

-- onSay
npcType.onSay = function(npc, creature, type, message)
    npcHandler:onSay(npc, creature, type, message)
end

-- onPlayerCloseChannel
npcType.onCloseChannel = function(npc, player)
    npcHandler:onCloseChannel(npc, player)
end

-- On buy npc shop message
npcType.onBuyItem = function(npc, player, id, subType, amount, ignore, inBackpacks, totalCost)
    -- todo: this is call after in c#, but in c++ this is call before
    -- npc:sellItem(player, id, amount, subType, 0, ignore, inBackpacks)
end

-- On sell npc shop message
npcType.onSellItem = function(npc, player, id, subtype, amount, ignore, name, totalCost)
    player:sendTextMessage(MESSAGE_INFO_DESCR, string.format("Sold %ix %s for %i gold.", amount, name, totalCost))
end

-- On check npc shop message (look item)
npcType.onCheckItem = function(npc, player, id, subType)
end

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
