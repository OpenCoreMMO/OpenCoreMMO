local talkAction = TalkAction("!test")

function talkAction.onSay(player, words, param)
    logger.info('executing talkAction test from lua: ' .. words .. ' ' .. param)

    -- local message = string.format("%s was KILLED at level %d by %s", player:getName(), 1, "test")
    -- sendChannelMessage(9, TALKTYPE_CHANNEL_R1, message)

    -- logger.info(player:getName())
    -- logger.info(player:getId())

    -- local loginStr = "TEST !!"
    -- player:sendTextMessage(MESSAGE_STATUS_DEFAULT, loginStr)

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
    ---------------------------------------------------------------------------

    -- CreatureEvent tests

    -- local bossTroll = Game.createMonster("Troll", player:getPosition())

    -- local onBossTrollThink = CreatureEvent("BossTrollOnThink")

    -- function onBossTrollThink.onThink(creature)
    --     if not creature then
    --         return true
    --     end

    --     local name = creature:getName()

    --     logger.info(name)

    --     creature:say("WAAAUUUUUUU10000", TALKTYPE_MONSTER_SAY)
    --     creature:getPosition():sendMagicEffect(CONST_ME_POFF)

    --     return true
    -- end

    -- onBossTrollThink:register()

    -- bossTroll:registerEvent("BossTrollOnThink")
    -- bossTroll:registerEvent("PlayerDeath")
    -- bossTroll:registerEvent("PlayerPrepareDeath")
    --------------------------------------------------------------------------

    -- Storage tests

    -- local storageValue = player:getStorageValue(1000)

    -- logger.info('storageValue: ' .. storageValue)

    -- if storageValue == -1 then
    --     player:setStorageValue(1000, 1996)
    --     storageValue = player:getStorageValue(1000)
    --     logger.info('storageValue2: ' .. storageValue)
    -- end
    ----------------------------------------------------------------------------

    -- Group tests

    -- local group = player:getGroup()

    -- logger.info('groupId: ' .. group:getId())
    -- logger.info('groupName: ' .. group:getName())
    -- logger.info('getAccess: ' .. tostring(group:getAccess()))

    -- logger.info('PlayerFlag_CanTalkRedChannel: ' .. tostring(group:hasFlag(PlayerFlag_CanTalkRedChannel)))
    -- logger.info('PlayerFlag_IgnoreYellCheck: ' .. tostring(group:hasFlag(PlayerFlag_IgnoreYellCheck)))
    -- logger.info('PlayerFlag_IgnoredByMonsters: ' .. tostring(group:hasFlag(PlayerFlag_IgnoredByMonsters)))
    -- logger.info('PlayerFlag_CannotBeAttacked: ' .. tostring(group:hasFlag(PlayerFlag_CannotBeAttacked)))

    ---------------------------------------------------------------------------

    -- Event tests

    -- addEvent(Game.broadcastMessage, 1000, "New record1: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    -- addEvent(Game.broadcastMessage, 2000, "New record2: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    -- addEvent(Game.broadcastMessage, 3000, "New record3: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)
    -- addEvent(Game.broadcastMessage, 4000, "New record4: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)

    -- local eventId = addEvent(Game.broadcastMessage, 5000, "New record5: " .. 0 .. " players are logged in.", MESSAGE_LOGIN)

    -- logger.info('eventId5 = ' .. tostring(eventId))

    -- stopEvent(eventId);

    -- Database tests
    -- logger.info("query")
    -- local resultQuery = db.query("SELECT * FROM Player")
    -- logger.info(tostring(resultQuery))

    -- logger.info("asyncQuery")
    -- local resultAsyncQuery = db.asyncQuery("SELECT * FROM Player")
    -- logger.info(tostring(resultAsyncQuery))

    -- logger.info("store")
    -- local resultStoreId = db.storeQuery("SELECT * FROM Player")
    -- logger.info(tostring(resultStoreId))

    -- if resultStoreId then
    --     repeat
    --         local id = Result.getNumber(resultStoreId, "id")
    --         local townId = Result.getNumber(resultStoreId, "townId")
    --         local name = Result.getString(resultStoreId, "name")
    --         logger.info(string.format("id: %d, townId: %d, name: %s", id, townId, name))
    --     until not Result.next(resultStoreId)

    --     Result.free(resultStoreId)
    -- end

    -- logger.info("asyncStore")
    -- local resultAsyncStoreId = db.storeQuery("SELECT * FROM Player")
    -- logger.info(tostring(resultAsyncStoreId))

    -- if resultAsyncStoreId then
    --     repeat
    --         local id = Result.getNumber(resultAsyncStoreId, "id")
    --         local townId = Result.getNumber(resultAsyncStoreId, "townId")
    --         local name = Result.getString(resultAsyncStoreId, "name")
    --         logger.info(string.format("id: %d, townId: %d, name: %s", id, townId, name))
    --     until not Result.next(resultAsyncStoreId)

    --     Result.free(resultAsyncStoreId)
    -- end

    --db.query("INSERT INTO worldrecords VALUES(2, 1, 2, '2025-01-04 03:36:26')")
    ---------------------------------------------------------------------------


    -- Container tests
    logger.info("Container tests")

    local container = player:getSlotItem(CONST_SLOT_BACKPACK)
    if not container then
        player:sendCancelMessage("You do not have a container in slot 0.")
        return false
    end

    logger.info("Container id: " .. container:getId())
    logger.info("Container size: " .. container:getSize())
    logger.info("Container capacity: " .. container:getCapacity())
    logger.info("Container emptySlots: " .. container:getEmptySlots())
    logger.info("Container contentDescription: " .. container:getContentDescription())
    logger.info("Container items: " .. #container:getItems())
    logger.info("Container item count: " .. container:getItemHoldingCount())
    logger.info("Container item count by id 2666 meat: " .. container:getItemCountById(2666))
    logger.info("Container item count by id 2175 spellbook: " .. container:getItemCountById(2175))
    local itemFromindex = container:getItem(0)
    logger.info("Container get item by index 0: " .. itemFromindex:getName())
    logger.info("Container has item: " .. tostring(container:hasItem(itemFromindex)))
    local itemFromAddItem = container:addItem(2120, 1) -- Add a rope to the container
    logger.info("Container has item after adding rope: " .. tostring(container:hasItem(itemFromAddItem)))
    local itemToAddItemEx = Game.createItem(2124, 1) -- Create a crystal ring item
    container:addItemEx(itemToAddItemEx) -- Add a crystal ring to the container
    logger.info("Container has item after adding crystal ring: " .. tostring(container:hasItem(itemToAddItemEx)))
    ---------------------------------------------------------------------------

    logger.info('end talkaction test from lua')
    return true
end

talkAction:separator(" ")
talkAction:register()
    