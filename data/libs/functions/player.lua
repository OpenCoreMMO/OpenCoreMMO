function Player.sendCancelMessage(self, message)
    if type(message) == "number" then
        message = Game.getReturnMessage(message)
    end
    return self:sendTextMessage(MESSAGE_STATUS_SMALL, message)
end

function Player.hasFlag(self, flag)
    return self:getGroup():hasFlag(flag)
end

function Player.removeTotalMoney(self, amount)
    local moneyCount = self:getMoney()
    local bankCount = self:getBankBalance()
    if amount <= moneyCount then
        self:removeMoney(amount)
        return true
    elseif amount <= (moneyCount + bankCount) then
        if moneyCount ~= 0 then
            self:removeMoney(moneyCount)
            local remains = amount - moneyCount
            self:setBankBalance(bankCount - remains)
            self:sendTextMessage(MESSAGE_INFO_DESCR,
                                 ("Paid %d from inventory and %d gold from bank account. Your account balance is now %d gold."):format(
                                     moneyCount, amount - moneyCount, self:getBankBalance()))
            return true
        end

        self:setBankBalance(bankCount - amount)
        self:sendTextMessage(MESSAGE_INFO_DESCR,
                             ("Paid %d gold from bank account. Your account balance is now %d gold."):format(
                                 amount, self:getBankBalance()))
        return true
    end
    return false
end