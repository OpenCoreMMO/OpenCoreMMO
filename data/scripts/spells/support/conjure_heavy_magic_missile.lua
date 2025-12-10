local spell = Spell("instant")

local BLANK_RUNE = 2260
local RUNE_ID = 2311 -- heavy magic missile rune
local CHARGES = 10

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

spell:name("Heavy Magic Missile")
spell:words("adori vis")
spell:group("support")
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:id(202)
spell:cooldown(2 * 1000)
spell:groupCooldown(2 * 1000)
spell:level(12)
spell:mana(120)
spell:soul(2)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
