local spell = Spell("instant")

local BLANK_RUNE = 2260
local RUNE_ID = 2313 -- explosion rune
local CHARGES = 6

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

spell:name("Explosion")
spell:words("adevo mas hur")
spell:group("support")
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:id(205)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(31)
spell:mana(570)
spell:soul(3)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
