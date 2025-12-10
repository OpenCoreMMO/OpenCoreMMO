local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    player:removeCondition(CONDITION_INVISIBLE)
    player:getPosition():sendMagicEffect(CONST_ME_POFF)
    return true
end

spell:name("Cancel Invisibility")
spell:words("exana ina")
spell:group("support")
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:id(52)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(26)
spell:mana(200)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
