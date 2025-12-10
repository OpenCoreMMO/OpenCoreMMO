local spell = Spell("instant")

local BLANK_RUNE = 2260
local RUNE_ID = 2315 -- thunderstorm rune
local CHARGES = 4

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    if not player:removeItem(BLANK_RUNE, 1) then
        player:sendTextMessage(MESSAGE_STATUS_SMALL, "You need a blank rune.")
        return false
    end

    player:addItem(RUNE_ID, CHARGES)
    player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)
    return true
end

spell:name("Thunderstorm Rune")
spell:words("adori mas vis")
spell:group("support")
spell:vocation("druid;true", "elder druid;true")
spell:id(219)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(28)
spell:mana(430)
spell:soul(3)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
