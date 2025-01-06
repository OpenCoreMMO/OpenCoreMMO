local yellowPillow = MoveEvent()

function yellowPillow.onStepIn(creature, item, position, fromPosition)
	logger.info("yellowPillow.onStepIn")
	if not creature:isPlayer() or creature:isInGhostMode() then
		return true
	end
	creature:say("Faaart!", TALKTYPE_MONSTER_SAY)
	item:getPosition():sendMagicEffect(CONST_ME_POFF)
	return true
end

yellowPillow:type("StepIn")
yellowPillow:id(8072)
yellowPillow:register()