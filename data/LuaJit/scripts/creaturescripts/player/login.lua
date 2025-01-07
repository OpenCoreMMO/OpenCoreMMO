local playerLogin = CreatureEvent("PlayerLogin")

function playerLogin.onLogin(player)
	-- Welcome
	local loginStr = "Welcome to OpenCoreMMO!!"
	player:sendTextMessage(MESSAGE_STATUS_DEFAULT, loginStr)

	-- Events
	player:registerEvent("ExtendedOpcode")
	player:registerEvent("PlayerDeath")
	player:registerEvent("PlayerAdvance")
	player:registerEvent("PlayerPrepareDeath")
	player:registerEvent("PlayerTextEdit")
	player:registerEvent("PlayerKill")
	-- player:registerEvent("DropLoot")

	--player:registerEvent("onThinkExample")
	--player:registerEvent("PlayerOnThinkTest")

	return true
end

playerLogin:register()
