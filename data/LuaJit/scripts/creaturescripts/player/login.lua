local playerLogin = CreatureEvent("PlayerLogin")

function playerLogin.onLogin(player)
	-- Welcome
	local loginStr = "Welcome to OpenCoreMMO!!"
	player:sendTextMessage(MESSAGE_STATUS_DEFAULT, loginStr)
	return true
end

playerLogin:register()
