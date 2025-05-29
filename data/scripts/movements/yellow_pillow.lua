local yellowPillow = MoveEvent()

function yellowPillow.onStepIn(creature, item, position, fromPosition)
	logger.info("yellowPillow.onStepIn2")
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

local test = MoveEvent()

local failPosition = Position(32060, 32192, 7)

function test.onStepIn(creature, item, position, fromPosition)
	logger.info("test.onStepIn")
	failPosition:sendMagicEffect(CONST_ME_MAGIC_BLUE)
	creature:teleportTo(failPosition)
	return true
end

test:type("StepIn")
test:uid(8800, 8801)
test:register()