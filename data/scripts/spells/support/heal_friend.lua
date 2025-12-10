local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local caster = Player(creature)
    if not caster then
        return false
    end

    local targetName = variant:getString()
    if targetName == nil or targetName == "" then
        caster:sendTextMessage(MESSAGE_STATUS_SMALL, "You need to specify a player.")
        return false
    end

    local target = Player(targetName)
    if not target then
        caster:sendTextMessage(MESSAGE_STATUS_SMALL, "Player is not online.")
        return false
    end

    local level = caster:getLevel()
    local mag = caster:getSkillLevel(SKILLTYPE_MAGIC)
    local min = (level / 5) + (mag * 1.1) + 8
    local max = (level / 5) + (mag * 1.5) + 14
    local amount = math.random(math.floor(min), math.floor(max))

    target:addHealth(amount)
    target:getPosition():sendMagicEffect(CONST_ME_MAGIC_GREEN)
    return true
end

spell:name("Heal Friend")
spell:words("exura sio")
spell:group("healing")
spell:vocation("druid;true", "elder druid;true")
spell:id(50)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(18)
spell:mana(70)
spell:hasParams(true)
spell:needLearn(false)
spell:isAggressive(false)
spell:isPremium(false)
spell:register()
