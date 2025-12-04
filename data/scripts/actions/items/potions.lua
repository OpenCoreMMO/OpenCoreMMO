 --local berserk = Condition(CONDITION_ATTRIBUTES)
 --berserk:setParameter(CONDITION_PARAM_TICKS, 10 * 60 * 1000)
 --berserk:setParameter(CONDITION_PARAM_SKILL_MELEE, 5)
 --berserk:setParameter(CONDITION_PARAM_SKILL_SHIELD, -10)
 --berserk:setParameter(CONDITION_PARAM_BUFF_SPELL, true)

 --local mastermind = Condition(CONDITION_ATTRIBUTES)
 --mastermind:setParameter(CONDITION_PARAM_TICKS, 10 * 60 * 1000)
 --mastermind:setParameter(CONDITION_PARAM_STAT_MAGICPOINTS, 3)
 --mastermind:setParameter(CONDITION_PARAM_BUFF_SPELL, true)

 --local bullseye = Condition(CONDITION_ATTRIBUTES)
 --bullseye:setParameter(CONDITION_PARAM_TICKS, 10 * 60 * 1000)
 --bullseye:setParameter(CONDITION_PARAM_SKILL_DISTANCE, 5)
 --bullseye:setParameter(CONDITION_PARAM_SKILL_SHIELD, -10)
 --bullseye:setParameter(CONDITION_PARAM_BUFF_SPELL, true)

 --local antidote = Combat()
 --antidote:setParameter(COMBAT_PARAM_TYPE, COMBAT_HEALING)
 --antidote:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_MAGIC_BLUE)
 --antidote:setParameter(COMBAT_PARAM_DISPEL, CONDITION_POISON)
 --antidote:setParameter(COMBAT_PARAM_AGGRESSIVE, false)
 --antidote:setParameter(COMBAT_PARAM_TARGETCASTERORTOPMOST, true)

 -- local exhaust = Condition(CONDITION_EXHAUST_HEAL)
 -- exhaust:setParameter(CONDITION_PARAM_TICKS, (configManager.getNumber(configKeys.EX_ACTIONS_DELAY_INTERVAL) - 1000))

 -- local function magicshield(player)
 -- 	local condition = Condition(CONDITION_MANASHIELD)
 -- 	condition:setParameter(CONDITION_PARAM_TICKS, 60000)
 -- 	condition:setParameter(CONDITION_PARAM_MANASHIELD, math.min(player:getMaxMana(), 300 + 7.6 * player:getLevel() + 7 * player:getMagicLevel()))
 -- 	exhaust:setParameter(CONDITION_PARAM_TICKS, 500)
 -- 	player:addCondition(condition)
 -- end

 local potions = {
 	[7588] = { health = { 250, 350 }, vocations = { VOCATION.BASE_ID.PALADIN, VOCATION.BASE_ID.KNIGHT }, level = 50, flask = 7634, description = "Only knights and paladins of level 50 or above may drink this fluid." },
 	[7589] = { mana = { 115, 185 }, level = 50, flask = 7634, description = "Only players of level 50 or above may drink this fluid." },
 	[7590] = { mana = { 150, 250 }, vocations = { VOCATION.BASE_ID.SORCERER, VOCATION.BASE_ID.DRUID, VOCATION.BASE_ID.PALADIN }, level = 80, flask = 7635, description = "Only sorcerers, druids and paladins of level 80 or above may drink this fluid." },
 	[7591] = { health = { 425, 575 }, vocations = { VOCATION.BASE_ID.KNIGHT }, level = 80, flask = 7635, description = "Only knights of level 80 or above may drink this fluid." },
 	[7618] = { health = { 125, 175 }, flask = 7636 },
 	[7620] = { mana = { 75, 125 }, flask = 7636 },
 	[6558] = { transform = { id = { 7588, 7589 } }, effect = CONST_ME_DRAWBLOOD },
 	--[7439] = { vocations = { VOCATION.BASE_ID.KNIGHT }, condition = berserk, effect = CONST_ME_MAGIC_RED, description = "Only knights may drink this potion.", text = "You feel stronger.", achievement = "Berserker" },
 	[7440] = { vocations = { VOCATION.BASE_ID.SORCERER, VOCATION.BASE_ID.DRUID }, condition = mastermind, effect = CONST_ME_MAGIC_BLUE, description = "Only sorcerers and druids may drink this potion.", text = "You feel smarter.", achievement = "Mastermind" },
 	--[7443] = { vocations = { VOCATION.BASE_ID.PALADIN }, condition = bullseye, effect = CONST_ME_MAGIC_GREEN, description = "Only paladins may drink this potion.", text = "You feel more accurate.", achievement = "Sharpshooter" },
 	[8472] = { health = { 250, 350 }, mana = { 100, 200 }, vocations = { VOCATION.BASE_ID.PALADIN }, level = 80, flask = 7635, description = "Only paladins of level 80 or above may drink this fluid." },

 	[8473] = { health = { 650, 850 }, vocations = { VOCATION.BASE_ID.KNIGHT }, level = 130, flask = 7635, description = "Only knights of level 130 or above may drink this fluid." },
 	--[8474] = { combat = antidote, flask = 7636 },
 	[8704] = { health = { 60, 90 }, flask = 7636 },
 }

 local flaskPotion = Action()

 function flaskPotion.onUse(player, item, fromPosition, target, toPosition, isHotkey)
 	if not target or type(target) == "userdata" and not target:isPlayer() then
 		return false
 	end

 	local potion = potions[item:getId()]
 	if not player:getGroup():getAccess() and (potion.level and player:getLevel() < potion.level or potion.vocations and not table.contains(potion.vocations, player:getVocation():getBaseId())) then
 		player:say(potion.description, TALKTYPE_MONSTER_SAY)
 		return true
 	end

 	if potion.health or potion.mana or potion.combat then
 		if potion.health then
 			doTargetCombatHealth(player, target, COMBAT_HEALING, potion.health[1], potion.health[2], CONST_ME_MAGIC_BLUE)
 		end

 		if potion.mana then
 			doTargetCombatMana(0, target, potion.mana[1], potion.mana[2], CONST_ME_MAGIC_BLUE)
 		end

 		if potion.combat then
 			potion.combat:execute(target, Variant(target:getId()))
 		end

 		if not potion.effect and target:getPosition() ~= nil then
 			target:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
 		end

 		--player:addAchievementProgress("Potion Addict", 100000)
 		
		target:say("Aaaah...", TALKTYPE_MONSTER_SAY)

 		-- local deactivatedFlasks = player:kv():get("talkaction.potions.flask") or false
 		-- if not deactivatedFlasks then
 		-- 	if fromPosition.x == CONTAINER_POSITION then
 		-- 		player:addItem(potion.flask, 1)
 		-- 	else
 		-- 		Game.createItem(potion.flask, 1, fromPosition)
 		-- 	end
 		-- end
 	end

 	--player:getPosition():sendSingleSoundEffect(SOUND_EFFECT_TYPE_ITEM_USE_POTION, player:isInGhostMode() and nil or player)

 	--if potion.func then
 	--	potion.func(player)
 	--	player:say("Aaaah...", MESSAGE_POTION)
 	--	player:getPosition():sendMagicEffect(potion.effect)
	 --
 	--if potion.achievement then
 	--		player:addAchievementProgress(potion.achievement, 100)
 	--	end
 	--end
	 --
 	--if potion.condition then
 	--	player:addCondition(potion.condition)
 	--	player:say(potion.text, MESSAGE_POTION)
 	--	player:getPosition():sendMagicEffect(potion.effect)
 	--end
	 --
 	--if potion.transform then
 	--	if item:getCount() >= 1 then
 	--		item:remove(1)
 	--		player:addItem(potion.transform.id[math.random(#potion.transform.id)], 1)
 	--		item:getPosition():sendMagicEffect(potion.effect)
 	--	    --player:addAchievementProgress("Demonic Barkeeper", 250)
 	--		return true
 	--	end
 	--end
	 --
 	--if not configManager.getBoolean(configKeys.REMOVE_POTION_CHARGES) then
 	--	return true
 	--end
	 --
 	----player:updateSupplyTracker(item)
 	item:remove(1)
 	return true
 end

 for index, value in pairs(potions) do
 	flaskPotion:id(index)
 end

 flaskPotion:register()
