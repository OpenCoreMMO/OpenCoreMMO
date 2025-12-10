local POISON = _G.CONDITION_POISONED or _G.CONDITION_POISON or 1

local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    player:removeCondition(CONDITION_BLEEDING)
    player:removeCondition(POISON)
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    return true
end

spell:name("Wound Cleansing")
spell:words("exana kor")
spell:group("healing")
spell:vocation(
    "sorcerer;true", "master sorcerer;true",
    "druid;true", "elder druid;true",
    "paladin;true", "royal paladin;true",
    "knight;true", "elite knight;true"
)
spell:id(7)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(20)
spell:mana(40)
spell:isAggressive(false)
spell:isSelfTarget(true)
spell:isPremium(false)
spell:needLearn(false)
spell:register()
