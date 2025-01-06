local hasteE = MoveEvent()

function hasteE.onEquip(creature, item, position, fromPosition)
	logger.info("hasteE.onEquip")
	if not creature:isPlayer() or creature:isInGhostMode() then
		return true
	end
	creature:say("hasteE.onEquip!", TALKTYPE_MONSTER_SAY)
	item:getPosition():sendMagicEffect(CONST_ME_POFF)
	return true
end

hasteE:type("equip")
hasteE:slot("feet")
hasteE:id(2195)
hasteE:level(10)
hasteE:register()

local hasteD = MoveEvent()

function hasteD.onDeEquip(creature, item, position, fromPosition)
	logger.info("hasteD.onDeEquipItem")
	if not creature:isPlayer() or creature:isInGhostMode() then
		return true
	end
	creature:say("hasteD.onDeEquipItem!", TALKTYPE_MONSTER_SAY)
	item:getPosition():sendMagicEffect(CONST_ME_POFF)
	return true
end

hasteD:type("deequip")
hasteD:slot("feet")
hasteD:id(2195)
hasteD:level(10)
hasteD:register()