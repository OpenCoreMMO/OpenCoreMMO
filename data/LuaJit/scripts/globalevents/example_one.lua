local Example_One = GlobalEvent("Example one")
function Example_One.onStartup()
	logger.info('GlobalEvent Example_One: onStartup')
	return true
end

Example_One:register()