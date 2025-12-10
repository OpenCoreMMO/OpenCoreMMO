local combat = Combat()
combat:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_MAGIC_BLUE)
combat:setParameter(COMBAT_PARAM_AGGRESSIVE, false)

-- In this codebase, the enum is exported as CONDITION_MANA_SHIELD (note the underscore).
local MANASHIELD = _G.CONDITION_MANA_SHIELD or _G.CONDITION_MANASHIELD or 512

local condition = Condition(MANASHIELD)
condition:setParameter(CONDITION_PARAM_TICKS, 200 * 1000) -- 200s, Tibia 8.6 padrão
combat:addCondition(condition)

local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local ok = combat:execute(creature, variant)
    if ok then
        creature:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    end
    return ok
end

spell:name("Magic Shield")
spell:words("utamo vita")
spell:group("support")
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:id(40)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(14)
spell:mana(50)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:isPremium(true)
spell:needLearn(false)
spell:register()
