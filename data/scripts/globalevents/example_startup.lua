local exampleStartup = GlobalEvent("ExampleStartup")
function exampleStartup.onStartup()
    logger.debug('GlobalEvent exampleStartup: onStartup')
    return true
end

exampleStartup:register()