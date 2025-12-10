local combat = Combat()
combat:setParameter(COMBAT_PARAM_TYPE, COMBAT_EARTHDAMAGE)
combat:setParameter(COMBAT_PARAM_EFFECT, CONST_ME_SMALLPLANTS)
combat:setParameter(COMBAT_PARAM_DISTANCEEFFECT, CONST_ANI_SMALLEARTH)
combat:setParameter(COMBAT_PARAM_CREATEITEM, ITEM_WILDGROWTH)

local rune = Spell("rune")

function rune.onCastSpell(creature, var, isHotkey)
    return combat:execute(creature, var)
end

rune:id(118)
rune:group("support")
rune:name("wild growth rune")
rune:runeId(2269)
rune:allowFarUse(true)
rune:setPzLocked(true)
rune:charges(2)
rune:level(27)
rune:magicLevel(5)
rune:cooldown(2 * 1000)
rune:groupCooldown(2 * 1000)
rune:isBlocking(true) -- True = Solid / False = Creature
rune:register()
