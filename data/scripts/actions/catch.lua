---@type Action
local catch = Action()

---@param player Player
local function test(player)
    player:getLevel()
end

---Gets a random number between min and max.
---@param min number
---@param max number
---@return number
function Player:getRandomNumber(min, max)
    min = min or 1
    max = max or 100
    return math.random(min, max)
end

---@param player Player
---@param pokemonName string
---@param pokeballItemId number
local function createPokeball(player, pokemonName, pokeballItemId)
	print('createPokeball init')

    local randomNumber = player:getRandomNumber(1, 100)
    
    print('randomNumber: ' .. randomNumber)

	local pokemon = MonsterType(pokemonName)
    
	if not pokemon then
		return false
	end

	local pokeball = player:addItem(pokeballItemId, 1)

	if not pokeball then
		return
	end

    pokeball:setCustomAttribute("PokemonName", pokemon:name())
	pokeball:setCustomAttribute("PokemonHealth", pokemon:maxHealth())
	pokeball:setCustomAttribute("PokemonMaxHealth", pokemon:maxHealth())
	
    print(pokeball:getCustomAttribute("PokemonName"))
    print(pokeball:getCustomAttribute("PokemonHealth"))
    print(pokeball:getCustomAttribute("PokemonMaxHealth"))

	print('createPokeball end')
end

local function catchEvent(player_uid, target_pos, chance, random, target_pokemon_name)
	print('catchEvent init')
	local player = Player(player_uid)

    print('aaaaaaaaaaaa')

    chance = chance + 100
    random = 0

    if chance >= random then
        player:getPosition():sendMagicEffect(EFFECT_CATCH_SUCCESS) -- no pokemon do player, efeito de sucesso
        player:sendTextMessage(27, "Congratulations, you caught a pokemon ("..target_pokemon_name..")!")
        --addEvent(player.addPokeball, 3000, player, transformPokeballOn, target_pokemon_name)
		--createPokeball(player, target_pokemon_name, 11315) --11315 pokeballId
		createPokeball(player, target_pokemon_name, 2176) --2176 orbId
    else
        player:getPosition():sendMagicEffect(EFFECT_CATCH_FAIL) -- no pokemon do player, efeito de fracasso
        player:sendTextMessage(27, "Sorry, you didn't catch that pokemon.")
    end
	print('catchEvent end')
end


---@param player Player
---@param item Item
---@param fromPosition Position
---@param target Thing
---@param toPosition Position
---@param isHotkey boolean
function catch.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    
    player:
    print('catch start')

    local tile_target = Tile(target:getPosition())
    if not tile_target then
        return true
    end

    local target_corps = tile_target:getTopDownItem()
    if not target_corps then
        return true
    end

    local target_id = target_corps:getId()

    print('target_id: ' .. target_id)

    local targetType = ItemType(target_id)
    if not targetType then
        return true
    end

    if not targetType:isCorpse() then
        return true
    end

    --local target_pokemon_name = target_corps:getAttribute(ITEM_ATTRIBUTE_NAME)
    local target_pokemon_name = target_corps:getName()

	target_pokemon_name = target_pokemon_name:gsub("^[Dd]ead%s*", "")
    target_pokemon_name = target_pokemon_name:sub(1, 1):upper() .. target_pokemon_name:sub(2)

    print('target_pokemon_name: ' .. target_pokemon_name)

    -- local target_pokemon_name = target_corps:getAttribute(ITEM_ATTRIBUTE_POKEMON_NAME)
    -- print(target_pokemon_name)

    if not target_pokemon_name then
        return true
    end

	local owner = target_corps:getAttribute(ITEM_ATTRIBUTE_CORPSEOWNER)
    
    print('player:getId() ' .. player:getId())
    print('owner: ' .. owner)

    if owner ~= player:getId() then
        player:sendCancelMessage("You are not allowed to catch this pokemon.")
        return true
    end


	local item_id = item:getId()

    print('item_id: ' .. item_id)

    local itemType = ItemType(item_id)

    if not itemType then
        return true
    end

	local target_pos = target_corps:getPosition()
    local player_pos = player:getPosition()

    print('target_pos: ' .. target_pos:toString())
    print('itemType getName: ' .. itemType:getName())

 	addEvent(catchEvent, 100, player.uid, target_pos, 0, 0, target_pokemon_name)

    -- local monsterType = MonsterType(target_pokemon_name)
    -- if not monsterType then
    --     return true
    -- end

    -- print(monsterType)
    -- print(monsterType:getName())

	-- local rate = configManager.getNumber(configKeys.RATE_CATCH)
    -- local stats = monsterType:getStats()

	-- print('rate: ' .. rate)
	-- print('stats:getCatchRate(): ' .. stats:getCatchRate())
	-- print('stats:getExperience(): ' .. stats:getExperience())
	-- print('stats:getHP(): ' .. stats:getHP())
	-- print('stats:getAttack(): ' .. stats:getAttack())
	-- print('stats:getDefense(): ' .. stats:getDefense())
	-- print('stats:getSpecialAttack(): ' .. stats:getSpecialAttack())
	-- print('stats:getSpecialDeffense(): ' .. stats:getSpecialDeffense())
	-- print('stats:getSpeed(): ' .. stats:getSpeed())
	-- print('stats:getHPFactor(): ' .. stats:getHPFactor())
	-- print('stats:getLevel(): ' .. stats:getLevel())
	-- print('stats:getWildLevel(): ' .. stats:getWildLevel())
	-- print('stats:getType1(): ' .. stats:getType1())
	-- print('stats:getType2(): ' .. stats:getType2())

	-- local target_pos = target_corps:getPosition()
    -- local player_pos = player:getPosition()

	-- local itemType_getCatchRate = 1 --TODO 
    -- local random = math.random()
    -- local chance = itemType_getCatchRate * (stats:getCatchRate() / 100)

	-- player_pos:sendDistanceEffect(target_pos, 48)

	-- if chance >= random then
    --     target_pos:sendMagicEffect(25) 
    -- else
    --     target_pos:sendMagicEffect(24) 
    -- end

	-- local distance = player_pos:getDistance(target_pos);
	-- local time = distance * 70 + 100 - (distance * 14);

    -- print('catch end, chance: ' .. chance .. ' random: ' .. random .. ' time: ' .. time)

 	-- addEvent(catchEvent, 4000, player.uid, target_pos, chance, random, target_pokemon_name)

	target_corps:remove(1)
	item:remove(1)

    print('catch end')
	return true
end

--catch:id(11310)
catch:id(2147) --small ruby
catch:register()
