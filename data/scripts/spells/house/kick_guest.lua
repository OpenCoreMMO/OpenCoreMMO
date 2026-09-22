local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local target = Player(variant:getString()) or creature
    local tile = target:getTile()
    local house = tile and tile:getHouse() or nil
    if not house or not house:kickPlayer(creature, target) then
        creature:sendCancelMessage(RETURNVALUE_NOTPOSSIBLE)
        creature:getPosition():sendMagicEffect(CONST_ME_POFF)
        return false
    end
    return true
end

spell:name("House Kick")
spell:words("alana sio")
spell:group("support")
spell:id(252)
spell:cooldown(1000)
spell:groupCooldown(1000)
spell:level(1)
spell:mana(0)
spell:hasParams(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
