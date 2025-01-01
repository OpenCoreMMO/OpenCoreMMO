local exampleShutdown = GlobalEvent("ExampleShutdown")
function exampleShutdown.onShutdown()
	logger.info('GlobalEvent exampleShutdown: onShutdown')
	return true
end

exampleShutdown:register()