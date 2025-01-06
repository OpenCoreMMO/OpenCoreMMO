--[[
    https://otland.net/members/snavy.155163/
    ####################################################################
    #   ____   ____  __  __ ____  ______ _____  __  __          _   _  #
    #  |  _ \ / __ \|  \/  |  _ \|  ____|  __ \|  \/  |   /\   | \ | | #
    #  | |_) | |  | | \  / | |_) | |__  | |__) | \  / |  /  \  |  \| | #
    #  |  _ <| |  | | |\/| |  _ <|  __| |  _  /| |\/| | / /\ \ | . ` | #
    #  | |_) | |__| | |  | | |_) | |____| | \ \| |  | |/ ____ \| |\  | #
    #  |____/ \____/|_|  |_|____/|______|_|  \_\_|  |_/_/    \_\_| \_| #
    ####################################################################
]]

BOMBERMAN_PLAYERS = {}
local config = {
    --| Where players will stand before starting the game.
    enterTiles = {
        from = Position(52, 424, 7),
        to   = Position(55, 424, 7)
    },
    ----------------------------------
    startPositions = {
        [1] = Position(47, 412, 7), -- top left     | 1 |   |   |   | 3 |
        [2] = Position(61, 422, 7), -- bottom right |   |   |   |   |   |
        [3] = Position(61, 412, 7), -- top right    |   |   |   |   |   |
        [4] = Position(47, 422, 7)  -- bottom left  | 4 |   |   |   | 2 |
    },
    ----------------------------------
    minPlayerLimit      = 1,
    maxPlayerLimit      = 4,
    ----------------------------------
    leverEnterAid       = 13442,
    barrelId            = 9468,
    ----------------------------------
    exhaustionDelay     = 1,     -- seconds
    playerMaxBombs      = 3,
    bombsAtStart        = 1,
    bombMaxRadius       = 5,
    bombRadius          = 1,
    bombDelay           = 3,     -- seconds
    bombAid             = 13443,
    ----------------------------------
    bonusBombChance     = 25,    -- %
    bonusRadiusChance   = 25,
    bonusBombAid        = 13444,
    bonusRadiusAid      = 13445,
    bonusPointAid       = 13447,
    ----------------------------------
    storagePoints       = 13448,
    storageInGame       = 13449,
    storageBombRadius   = 13450,
    storagePlayerBombs  = 13451,
    storageTrackBombs   = 13452,
    storageExhaustion   = 13453,
    ----------------------------------
    countDownEffect    = CONST_ME_ENERGYAREA,
    explodeEffect      = CONST_ME_FIREAREA,
}

-----------------------[ LOCAL FUNCTIONS ]----------------------
local function positionToReadable(p)
    return "( ".. p.x .." / ".. p.y .." / ".. p.z .." )"
end

local function validateEnterTiles()
    -- if both x & y are inequal, FROM & TO does not make a straight line.
    if config.enterTiles.from.x ~= config.enterTiles.to.X
    and config.enterTiles.from.y ~= config.enterTiles.to.y then
        return false
    end
    return true
end

local function getPlayersWaiting()
    local players = {}
    local z = config.enterTiles.from.z
    for x = config.enterTiles.from.x, config.enterTiles.to.x do
        for y = config.enterTiles.from.y, config.enterTiles.to.y do
            local tile = Tile(x, y, z)
            if not tile then
                print('[Error - Bomberman::countPlayersToEnter] Tile not found ('.. positionToReadable(Position(x, y, z)) ..')')
                return false
            end
            local creature = tile:getTopCreature()
            if creature and creature:isPlayer() then
                table.insert(players, creature)
            end
        end
    end
    return players
end
-------------------------[ Action ]-----------------------------
local bombEnter = Action('bombermanEnter')
function bombEnter.onUse(player, item, position, fromPosition)
    if not validateEnterTiles() then
        player:say('! ERROR !', TALKTYPE_MONSTER_SAY)
        print('[Error - Bomberman::Action::onUse] Tile positions are incorrect.')
        return true
    end

    local playersWaiting = getPlayersWaiting()
    if not playersWaiting then
        player:say('! ERROR !', TALKTYPE_MONSTER_SAY, false, nil, item:getPosition())
        return true
    end

    if #playersWaiting < config.minPlayerLimit then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, 'Atleast ' .. config.minPlayerLimit .. ' players need to join.')
        return true
    end

    BOMBERMAN_PLAYERS = {}
    -- Telepot players to the game
    for i, position in ipairs(config.startPositions) do
        local p = playersWaiting[i]
        if p then
            p:teleportTo(position)
            p:setStorageValue(config.storageInGame, 1)
            p:setStorageValue(config.storageBombRadius, config.bombRadius)
            p:setStorageValue(config.storagePlayerBombs, config.bombsAtStart)
            p:setStorageValue(config.storageTrackBombs, config.bombsAtStart)
            BOMBERMAN_PLAYERS[p:getId()] = p
        end
    end
    return true
end
bombEnter:aid(config.leverEnterAid)
bombEnter:register()

-------------------------[ TalkAction ]-----------------------------
local function resetArena()
    BOMBERMAN_PLAYERS = {}
    local ignorePositions = {
        config.startPositions[1],
        Position(config.startPositions[1].x + 1, config.startPositions[1].y,     config.startPositions[1].z),
        Position(config.startPositions[1].x,     config.startPositions[1].y + 1, config.startPositions[1].z),
        config.startPositions[2],
        Position(config.startPositions[2].x - 1, config.startPositions[2].y,     config.startPositions[2].z),
        Position(config.startPositions[2].x,     config.startPositions[2].y - 1, config.startPositions[2].z),
        config.startPositions[3],
        Position(config.startPositions[3].x - 1, config.startPositions[3].y,     config.startPositions[3].z),
        Position(config.startPositions[3].x,     config.startPositions[3].y + 1, config.startPositions[3].z),
        config.startPositions[4],
        Position(config.startPositions[4].x + 1, config.startPositions[4].y,     config.startPositions[4].z),
        Position(config.startPositions[4].x,     config.startPositions[4].y - 1, config.startPositions[4].z),
    }

    local z = config.startPositions[1].z
    for x = config.startPositions[1].x, config.startPositions[2].x do
        for y = config.startPositions[1].y, config.startPositions[2].y do
            local position = Position(x, y, z)
            local tile = Tile(position)
            if tile then
                tile:getGround():setActionId(0)
                if tile:isWalkable() and not table.contains(ignorePositions, position) then
                    if not Game.createItem(config.barrelId, 1, position) then
                        print('could not create at ' .. positionToReadable(position))
                    end
                end
            end
        end
    end
end

local function checkWinner()
    local count = 0
    local lastPlayer = nil
    for cid, player in pairs(BOMBERMAN_PLAYERS) do
        if player then
            count = count + 1
            lastPlayer = player
        end
    end

    if not lastPlayer then
        addEvent(resetArena, 200)
        return
    end

    if lastPlayer and count == 1 then
        lastPlayer:sendTextMessage(MESSAGE_STATUS_CONSOLE_ORANGE, 'You have won!')
        lastPlayer:teleportTo(lastPlayer:getTown():getTemplePosition())
        addEvent(resetArena, 200)
    end
end

local function showBonusBomb(position)
    local tile = Tile(position)
    if not tile then return end
    local ground = tile:getGround()
    if ground:getActionId() ~= config.bonusBombAid then return end
    position:sendMagicEffect(CONST_ME_FIREWORK_YELLOW)
    addEvent(showBonusBomb, 500, position)
end

local function showBonusRadius(position)
    local tile = Tile(position)
    if not tile then return end
    local ground = tile:getGround()
    if ground:getActionId() ~= config.bonusRadiusAid then return end
    position:sendMagicEffect(CONST_ME_FIREWORK_BLUE)
    addEvent(showBonusRadius, 500, position)
end

local function checkTiles(bombRadius, killer, position)
    local directions = {'WEST','EAST','NORTH','SOUTH'}
    for _, direction in pairs(directions) do
        for i = 0, bombRadius do
            local f = (function()
            --------------------------------------------------------
            local tile
            if direction == 'WEST'  then tile = Tile(position.x - i, position.y,     position.z) end
            if direction == 'EAST'  then tile = Tile(position.x + i, position.y,     position.z) end
            if direction == 'NORTH' then tile = Tile(position.x,     position.y - i, position.z) end
            if direction == 'SOUTH' then tile = Tile(position.x,     position.y + i, position.z) end
            if not tile then
                return false
            end

            local barrel = tile:getItemById(config.barrelId)
            if tile:getItemById(config.barrelId) then
                local tilePos = tile:getPosition()
                tilePos:sendMagicEffect(CONST_ME_GROUNDSHAKER)
                if math.random(1, 100) <= config.bonusBombChance then
                    tile:getGround():setActionId(config.bonusBombAid)
                    showBonusBomb(tilePos)
                elseif math.random(1, 100) <= config.bonusRadiusChance then
                    tile:getGround():setActionId(config.bonusRadiusAid)
                    showBonusRadius(tilePos)
                end
                barrel:remove()
                return false
            end

            if not tile:isWalkable() then
                return false
            end

            local topCreature = tile:getTopCreature()
            if topCreature and topCreature:isPlayer() then
                killer = Player(killer)
                local cname = topCreature:getName()
                local deadMsg = cname .. ' was killed by '
                -- killer might have died earlier & logged out.
                if not killer then
                    deadMsg = deadMsg .. 'a bomb.'
                else
                    if cname == killer:getName() then
                        deadMsg = deadMsg .. (topCreature:getSex() == PLAYERSEX_FEMALE and 'her' or 'him') .. 'self'
                    else
                        deadMsg = deadMsg .. killer:getName()
                    end
                    deadMsg = deadMsg .. ' in bomberman.'
                end

                topCreature:teleportTo(topCreature:getTown():getTemplePosition())
                topCreature:setStorageValue(config.storageInGame, -1)
                for _, player in pairs(BOMBERMAN_PLAYERS) do
                    if player then
                        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, deadMsg)
                    end
                end

                BOMBERMAN_PLAYERS[topCreature:getId()] = nil
                checkWinner()
            end

            tile:getPosition():sendMagicEffect(config.explodeEffect)
            return true
            --------------------------------------------------------
            end)()
            if not f then
                break
            end
        end
    end
end

local function detonateBomb(position, n)
    if n <= 0 then
        position:sendMagicEffect(config.explodeEffect)
        local bombTile   = Tile(position)
        local ground     = bombTile:getGround()
        local bombRadius = ground:getCustomAttribute('bombRadius')
        local killer     = ground:getCustomAttribute('killerId')
        local p = Player(killer)
        if p then
            p:setStorageValue(config.storagePlayerBombs, p:getStorageValue(config.storagePlayerBombs) + 1)
        end
        checkTiles(bombRadius, killer, position)
        ground:setActionId(0)
        return
    end
    position:sendMagicEffect(config.countDownEffect)
    addEvent(detonateBomb, 1000, position, n-1)
end

local ta = TalkAction('!bomb')
function ta.onSay(player, words, param)
    local taExhaustion = player:getStorageValue(config.storageExhaustion)
    if taExhaustion > os.time() then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, 'Slow down.')
        return false
    end

    if player:getStorageValue(config.storageInGame) <= 0 then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, 'You may not place bombs outside of bomberman game.')
        return false
    end

    local position = player:getPosition()
    local tile = Tile(position)
    if not tile then -- probably not necessary but JUSTINCASE
        print('[Error - bomberman::TalkAction] Tile not found ' .. positionToReadable(position))
        player:sendTextMessage(MESSAGE_STATUS_SMALL, '!Error!')
        player:teleportTo(player:getTown():getTemplePosition())
        return false
    end

    local ground = tile:getGround()
    if ground:getActionId() == config.bombAid then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, 'You cannot place two bombs on the same tile.')
        return false
    end

    local playerBombsLeft = player:getStorageValue(config.storagePlayerBombs)
    if playerBombsLeft <= 0 then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, 'You have no bombs left!')
        return false
    end

    local bombRadius = config.bombRadius
    local playerBombRadius = player:getStorageValue(config.storageBombRadius)
    if playerBombRadius > 0 then
        bombRadius = playerBombRadius
    end

    player:setStorageValue(config.storageExhaustion, os.time() + config.exhaustionDelay)
    player:setStorageValue(config.storagePlayerBombs, playerBombsLeft - 1)
    ground:setActionId(config.bombAid)
    ground:setCustomAttribute('bombRadius', bombRadius)
    ground:setCustomAttribute('killerId', player:getId())
    detonateBomb(position, config.bombDelay)
    return false
end
ta:separator(' ')
ta:register()
------------------------[ MoveEvent ]------------------------
local me = MoveEvent('bombermanBonusRadius')
function me.onStepIn(player, item, position, fromPosition)
    if item:getActionId() == config.bonusRadiusAid then
        local playerBombRadius = player:getStorageValue(config.storageBombRadius)
        if playerBombRadius < config.bombMaxRadius then
            player:setStorageValue(config.storageBombRadius, playerBombRadius + 1)
        end
        position:sendMagicEffect(CONST_ME_YELLOWENERGY)
    else
        local playerBombs = player:getStorageValue(config.storageTrackBombs)
        if playerBombs < config.playerMaxBombs then
            player:setStorageValue(config.storagePlayerBombs, playerBombs + 1)
            player:setStorageValue(config.storageTrackBombs, playerBombs + 1)
        end
        position:sendMagicEffect(CONST_ME_PURPLEENERGY)
    end
    item:setActionId(0)
    return true
end

me:aid(config.bonusRadiusAid, config.bonusBombAid)
me:register()