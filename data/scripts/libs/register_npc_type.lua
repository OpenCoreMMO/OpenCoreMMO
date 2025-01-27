registerNpcType = {}
setmetatable(registerNpcType, {
	__call = function(self, npcType, mask)
		for _, parse in pairs(self) do
			parse(npcType, mask)
		end
	end,
})

NpcType.register = function(self, mask)
	return registerNpcType(self, mask)
end

registerNpcType.name = function(npcType, mask)
	if mask.name then
		npcType:name(mask.name)
	end
end

registerNpcType.description = function(npcType, mask)
	if mask.description then
		npcType:nameDescription(mask.description)
	end
end

registerNpcType.outfit = function(npcType, mask)
	if mask.outfit then
		npcType:outfit(mask.outfit)
	end
end

registerNpcType.maxHealth = function(npcType, mask)
	if mask.maxHealth then
		npcType:maxHealth(mask.maxHealth)
	end
end

registerNpcType.health = function(npcType, mask)
	if mask.health then
		npcType:health(mask.health)
	end
end

-- registerNpcType.race = function(npcType, mask)
-- 	if mask.race then
-- 		npcType:race(mask.race)
-- 	end
-- end

registerNpcType.walkInterval = function(npcType, mask)
	if mask.walkInterval then
		npcType:walkInterval(mask.walkInterval)
	end
end

registerNpcType.walkRadius = function(npcType, mask)
	if mask.walkRadius then
		npcType:walkRadius(mask.walkRadius)
	end
end

registerNpcType.speed = function(npcType, mask)
	if mask.speed then
		npcType:baseSpeed(mask.speed)
	end
end

-- registerNpcType.flags = function(npcType, mask)
-- 	if mask.flags then
-- 		if mask.flags.floorchange ~= nil then
-- 			npcType:floorChange(mask.flags.floorchange)
-- 		end
-- 		if mask.flags.canPushCreatures ~= nil then
-- 			npcType:canPushCreatures(mask.flags.canPushCreatures)
-- 		end
-- 		if mask.flags.canPushItems ~= nil then
-- 			npcType:canPushItems(mask.flags.canPushItems)
-- 		end
-- 		if mask.flags.pushable ~= nil then
-- 			npcType:isPushable(mask.flags.pushable)
-- 		end
-- 	end
-- end

-- registerNpcType.light = function(npcType, mask)
-- 	if mask.light then
-- 		if mask.light.color then
-- 			local color = mask.light.color
-- 		end
-- 		if mask.light.level then
-- 			npcType:light(color, mask.light.level)
-- 		end
-- 	end
-- end

-- registerNpcType.respawnType = function(npcType, mask)
-- 	if mask.respawnType then
-- 		if mask.respawnType.period then
-- 			npcType:respawnTypePeriod(mask.respawnType.period)
-- 		end
-- 		if mask.respawnType.underground then
-- 			npcType:respawnTypeIsUnderground(mask.respawnType.underground)
-- 		end
-- 	end
-- end

-- registerNpcType.sounds = function(npcType, mask)
-- 	if type(mask.sounds) == "table" then
-- 		if mask.sounds.ticks and mask.sounds.chance and mask.sounds.ids and type(mask.sounds.ids) == "table" and #mask.sounds.ids > 0 then
-- 			npcType:soundSpeedTicks(mask.sounds.ticks)
-- 			npcType:soundChance(mask.sounds.chance)
-- 			for _, v in pairs(mask.sounds.ids) do
-- 				npcType:addSound(v)
-- 			end
-- 		end
-- 	end
-- end

-- registerNpcType.voices = function(npcType, mask)
-- 	if type(mask.voices) == "table" then
-- 		local interval, chance
-- 		if mask.voices.interval then
-- 			interval = mask.voices.interval
-- 		end
-- 		if mask.voices.chance then
-- 			chance = mask.voices.chance
-- 		end
-- 		for k, v in pairs(mask.voices) do
-- 			if type(v) == "table" then
-- 				npcType:addVoice(v.text, interval, chance, v.yell)
-- 			end
-- 		end
-- 	end
-- end

registerNpcType.voices = function(npcType, mask)
    if type(mask.voices) == "table" then
        local interval = mask.voices.interval or 15000
        local chance = mask.voices.chance or 50

		 local args = { interval, chance }

		 for _, v in ipairs(mask.voices) do
			 if type(v) == "table" and v.text then
				 table.insert(args, v.yell or false)
				 table.insert(args, v.text)
			 end
		 end
 
		 npcType:addVoices(table.unpack(args))
    end
end

-- registerNpcType.events = function(npcType, mask)
-- 	if type(mask.events) == "table" then
-- 		for k, v in pairs(mask.events) do
-- 			npcType:registerEvent(v)
-- 		end
-- 	end
-- end

-- -- Global item tracker to track buy and sell prices across all NPCs
NpcPriceChecker = NpcPriceChecker or {}

registerNpcType.shop = function(npcType, mask)
	if type(mask.shop) == "table" then
		for _, shopItems in pairs(mask.shop) do
			npcType:addShopItem(
				shopItems.id,
				shopItems.buy,
				shopItems.sell
			)
		end
	end
end

-- registerNpcType.currency = function(npcType, mask)
-- 	if mask.currency then
-- 		npcType:currency(mask.currency)
-- 	end
-- end

-- registerNpcType.speechBubble = function(npcType, mask)
-- 	if mask.speechBubble then
-- 		npcType:speechBubble(mask.speechBubble)
-- 	end
-- end
