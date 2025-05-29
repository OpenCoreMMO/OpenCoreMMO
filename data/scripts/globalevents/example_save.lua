local exampleSave = GlobalEvent("ExampleSave")
function exampleSave.onSave()
	logger.info('GlobalEvent exampleSave: onSave')
	return true
end

exampleSave:register()