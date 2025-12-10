local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local caster = Player(creature)
    if not caster then
        return false
    end

    local level = caster:getLevel()
    -- heal scale only with level to avoid missing skill enums in this env
    local min = (level / 3) + 30
    local max = (level / 2) + 60
    local heal = math.random(math.floor(min), math.floor(max))

    local origin = caster:getPosition()

    -- Editable area mask (1/x = apply, 0 = skip). 'x' marks the caster tile.
    local areaMask = {
        "000111000",
        "001111100",
        "011111110",
        "0111x1110",
        "011111110",
        "001111100",
        "000111000"
    }

    local midY = math.floor(#areaMask / 2) + 1
    for y = 1, #areaMask do
        local row = areaMask[y]
        local midX = math.floor(#row / 2) + 1
        for x = 1, #row do
            local cell = row:sub(x, x)
            if cell == "1" or cell == "x" then
                local dx = x - midX
                local dy = y - midY
                local pos = Position(origin.x + dx, origin.y + dy, origin.z)
                pos:sendMagicEffect(CONST_ME_MAGIC_GREEN)

                local tile = Tile(pos)
                if tile then
                    local target = tile:getTopCreature()
                    if target and target:isPlayer() then
                        target:addHealth(heal)
                    end
                end
            end
        end
    end

    return true
end

spell:name("Mass Healing")
spell:words("exura gran mas res")
spell:group("healing")
spell:vocation("druid;true", "elder druid;true")
spell:id(51)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(36)
spell:mana(150)
spell:isSelfTarget(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
