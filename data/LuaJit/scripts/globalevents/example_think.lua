local exampleThink = GlobalEvent("ExampleThink")
function exampleThink.onThink(interval)
	logger.info('GlobalEvent exampleThink: onThink')
	return true
end

exampleThink:interval(10000) -- 10 minutes interval
exampleThink:register()