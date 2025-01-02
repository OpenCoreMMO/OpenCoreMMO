local exampleStartup = GlobalEvent("ExampleStartup")
function exampleStartup.onStartup()
	logger.info('GlobalEvent exampleStartup: onStartup')
	return true
end

exampleStartup:register()