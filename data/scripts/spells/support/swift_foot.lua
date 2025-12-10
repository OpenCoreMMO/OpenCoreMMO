local combat = Combat()
combat:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_MAGIC_GREEN)
combat:setParameter(COMBAT_PARAM_AGGRESSIVE, false)

local condition = Condition(CONDITION_HASTE)
condition:setParameter(CONDITION_PARAM_TICKS, 22000)
condition:setFormula(1.1, 60, 1.1, 60)
combat:addCondition(condition)

local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local ok = combat:execute(creature, variant)
    if ok and creature then
        creature:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    end
    return ok
end

spell:name("Swift Foot")
spell:words("utani tempo hur")
spell:group("support")
spell:vocation("knight;true", "elite knight;true")
spell:id(64)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(20)
spell:mana(60)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:isPremium(false)
spell:needLearn(false)
spell:register()
