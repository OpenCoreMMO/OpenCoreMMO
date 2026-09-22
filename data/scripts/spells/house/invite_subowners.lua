local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    local tile = player:getTile()
    local house = tile and tile:getHouse() or nil
    if not house or not house:canEditAccessList(SUBOWNER_LIST, player) then
        player:sendCancelMessage(RETURNVALUE_NOTPOSSIBLE)
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return false
    end

    player:setEditHouse(house, SUBOWNER_LIST)
    player:sendHouseWindow(house, SUBOWNER_LIST)
    return true
end

spell:name("House Subowner List")
spell:words("aleta som")
spell:group("support")
spell:id(251)
spell:cooldown(1000)
spell:groupCooldown(1000)
spell:level(1)
spell:mana(0)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
