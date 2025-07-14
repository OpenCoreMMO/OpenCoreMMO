function Player.sendCancelMessage(self, message)
    if type(message) == "number" then
        message = Game.getReturnMessage(message)
    end
    return self:sendTextMessage(MESSAGE_STATUS_SMALL, message)
end

function Player.hasFlag(self, flag)
	return self:getGroup():hasFlag(flag)
end