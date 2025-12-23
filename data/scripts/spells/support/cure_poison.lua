local POISON = _G.CONDITION_POISONED or _G.CONDITION_POISON or 1

local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    player:removeCondition(POISON)
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    return true
end

spell:name("Cure Poison")
spell:words("exana pox")
spell:group("healing")
spell:vocation(
        "sorcerer;true", "master sorcerer;true",
        "druid;true", "elder druid;true",
        "paladin;true", "royal paladin;true",
        "knight;true", "elite knight;true"
)
spell:id(5)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(8)
spell:mana(30)
spell:isAggressive(false)
spell:isSelfTarget(true)
spell:needLearn(false)
spell:register()
