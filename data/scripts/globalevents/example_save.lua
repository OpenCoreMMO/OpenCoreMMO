local exampleSave = GlobalEvent("ExampleSave")
function exampleSave.onSave()
    logger.debug('GlobalEvent exampleSave: onSave')
    return true
end

exampleSave:register()