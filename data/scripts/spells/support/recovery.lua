local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    if not creature then
        return false
    end

    local level = creature:getLevel()
    -- Knight recovery: heal scaled by level only (no magic skill exposed here)
    local min = (level / 4) + 40
    local max = (level / 3) + 80
    local heal = math.random(math.floor(min), math.floor(max))

    creature:addHealth(heal)
    creature:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    return true
end

spell:name("Recovery")
spell:words("exura ico")
spell:group("healing")
spell:vocation("knight;true", "elite knight;true")
spell:id(63)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(20)
spell:mana(40)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
