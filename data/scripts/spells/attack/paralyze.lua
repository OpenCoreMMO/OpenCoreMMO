local combat = Combat()
combat:setParameter(COMBAT_PARAM_TYPE, COMBAT_UNDEFINEDDAMAGE)
combat:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_MAGIC_GREEN)
combat:setParameter(COMBAT_PARAM_DISTANCEEFFECT, CONST_ANI_SMALLEARTH)
combat:setParameter(COMBAT_PARAM_AGGRESSIVE, true)

-- O motor usa HASTE para cálculos de speed; fórmula negativa aplica paralyze sem depender de Condition.Value
local condition = Condition(CONDITION_HASTE)
condition:setParameter(CONDITION_PARAM_TICKS, 20000)
condition:setFormula(-0.7, 0, -0.9, 0) -- redução de speed
combat:addCondition(condition)

local spell = Spell("instant")

function spell.onCastSpell(creature, var)
    return combat:execute(creature, var)
end

spell:id(241)
spell:name("Paralyze")
spell:words("utori mort")
spell:group("attack")
spell:needTarget(true)
spell:allowFarUse(true)
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:level(54)
spell:mana(140)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:isAggressive(true)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
