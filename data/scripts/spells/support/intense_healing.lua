local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    local level = player:getLevel()
    local mag = player:getSkillLevel(SKILLTYPE_MAGIC)
    local min = (level / 5) + (mag * 0.4) + 20
    local max = (level / 5) + (mag * 0.8) + 40
    local amount = math.random(math.floor(min), math.floor(max))

    player:addHealth(amount)
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    return true
end

spell:name("Intense Healing")
spell:words("exura gran")
spell:group("healing")
spell:vocation(
        "sorcerer;true", "master sorcerer;true",
        "druid;true", "elder druid;true",
        "paladin;true", "royal paladin;true",
        "knight;true", "elite knight;true"
)
spell:id(3)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(20)
spell:mana(70)
spell:isAggressive(false)
spell:isSelfTarget(true)
spell:needLearn(false)
spell:register()
