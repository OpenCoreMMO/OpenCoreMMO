local talkAction = TalkAction("/s")

function talkAction.onSay(player, words, param)
    -- if not player:getGroup():getAccess() then
    -- 	return true
    -- end

    -- if player:getAccountType() < ACCOUNT_TYPE_GOD then
    -- 	return false
    -- end

    local position = player:getPosition()
    local monster = Game.createMonster(param, position, false, false, player)
    if monster then
        monster:getPosition():sendMagicEffect(CONST_ME_TELEPORT)
        position:sendMagicEffect(CONST_ME_MAGIC_RED)
    else
        player:sendCancelMessage("There is not enough room.")
        position:sendMagicEffect(CONST_ME_POFF)
    end
    return true
end

talkAction:separator(" ")
talkAction:register()
