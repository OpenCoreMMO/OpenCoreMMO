local spell = Spell("instant")

local BLANK_RUNE = 2260
local RUNE_ID = 2274 -- avalanche rune
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

spell:name("Avalanche Rune")
spell:words("adevo mas frigo")
spell:group("support")
spell:vocation("druid;true", "elder druid;true")
spell:id(217)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(30)
spell:mana(530)
spell:soul(5)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
