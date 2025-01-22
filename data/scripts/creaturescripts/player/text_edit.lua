local playerTextEdit = CreatureEvent("PlayerTextEdit")

function playerTextEdit.onTextEdit(player, item, text)
	logger.info('playerTextEdit.onTextEdit')
	return true
end

playerTextEdit:register()
