local combat = Combat()
combat:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_MAGIC_GREEN)
combat:setParameter(COMBAT_PARAM_AGGRESSIVE, 0)

local condition = Condition(CONDITION_HASTE)
condition:setParameter(CONDITION_PARAM_TICKS, 22000)
condition:setFormula(1.7, 40, 1.7, 40)
combat:addCondition(condition)

local spell = Spell("instant")

function spell.onCastSpell(creature, var)
    return combat:execute(creature, var)
end

spell:name("Strong Haste")
spell:words("utani gran hur")
spell:group("support")
spell:vocation("druid;true", "elder druid;true", "sorcerer;true", "master sorcerer;true")
spell:id(39)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(20)
spell:mana(100)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:isPremium(true)
spell:needLearn(false)
spell:register()
