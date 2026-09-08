local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local player = Player(creature)
    if not player then
        return false
    end

    local function tryDoor(pos)
        if not pos then
            return nil, nil
        end
        local tile = Tile(pos)
        local house = tile and tile:getHouse()
        if not house then
            return nil, nil
        end
        return house, house:getDoorIdByPosition(pos)
    end

    -- Facing tile first, then the caster's tile (standing in the doorway).
    -- House is resolved from the door tile so facing from outside still works.
    local house, doorId = tryDoor(variant:getPosition())
    if doorId == nil then
        house, doorId = tryDoor(player:getPosition())
    end

    if doorId == nil or not house:canEditAccessList(doorId, player) then
        player:sendCancelMessage(RETURNVALUE_NOTPOSSIBLE)
        player:getPosition():sendMagicEffect(CONST_ME_POFF)
        return false
    end

    player:setEditHouse(house, doorId)
    player:sendHouseWindow(house, doorId)
    return true
end

spell:name("House Door List")
spell:words("aleta grav")
spell:group("support")
spell:id(253)
spell:cooldown(1000)
spell:groupCooldown(1000)
spell:level(1)
spell:mana(0)
spell:isAggressive(false)
spell:needCasterTargetOrDirection(true)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
