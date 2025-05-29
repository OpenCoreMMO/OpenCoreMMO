local snow = MoveEvent()

function snow.onStepOut(creature, item, position, fromPosition)
	logger.info("snow.onStepOut")
	if not creature:isPlayer() or creature:isInGhostMode() then
		return true
	end

	if item:getId() == 670 then
		item:transform(6594)
	else
		item:transform(item.itemid + 15)
	end
	-- creature:addAchievementProgress("Snowbunny", 10000)
	item:decay()
	return true
end


snow:type("StepOut")
snow:id(670)
snow:idRange(6580, 6593)
snow:register()