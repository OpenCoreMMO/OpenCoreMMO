-- function Game.broadcastMessage(message, messageType)
-- 	if not messageType then
-- 		messageType = MESSAGE_GAME_HIGHLIGHT
-- 	end

-- 	for _, player in ipairs(Game.getPlayers()) do
-- 		player:sendTextMessage(messageType, message)
-- 	end
-- end