-- local decay = MoveEvent()

-- function decay.onStepIn(creature, item, position, fromPosition)
-- 	logger.info("decay.onStepIn")

-- 	if not creature:isPlayer() or creature:isInGhostMode() then
-- 		return true
-- 	end

-- 	item:transform(item.itemid + 1)
-- 	item:decay()
-- 	return tru
-- end


-- decay:type("StepIn")
-- decay:id(293, 461, 3310)
-- decay:register()