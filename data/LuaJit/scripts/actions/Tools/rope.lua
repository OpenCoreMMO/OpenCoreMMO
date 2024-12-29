local rope = Action()

local ropeSpots = { 384, 418, 8278, 8592 }

local holeId = { -- usable rope holes, for rope spots see global.lua
    294, 369, 370, 383, 392, 408, 409, 410, 427, 428, 429, 430, 462, 469, 470, 482,
    484, 485, 489, 924, 1369, 3135, 3136, 4835, 4837, 7933, 7938, 8170, 8249, 8250,
    8251, 8252, 8254, 8255, 8256, 8276, 8277, 8279, 8281, 8284, 8285, 8286, 8323,
    8567, 8585, 8595, 8596, 8972, 9606, 9625
}

function rope.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local tile = Tile(toPosition)
    if not tile then
        return false
    end

    local ground = tile:getGround()

    if ground and table.contains(ropeSpots, ground:getId()) then
        tile = Tile(toPosition:moveUpstairs())
        if not tile then
            return false
        end

        if tile:hasFlag(TILESTATE_PROTECTIONZONE) and player:isPzLocked() then
            player:sendCancelMessage(RETURNVALUE_PLAYERISPZLOCKED)
            return true
        end

        player:teleportTo(toPosition, false)
        return true
    end

    if table.contains(holeId, target.itemid) then
        toPosition.z = toPosition.z + 1
        tile = Tile(toPosition)
        if not tile then
            return false
        end

        local thing = tile:getTopVisibleThing()
        if not thing then
            return true
        end

        if thing:isPlayer() then
            if Tile(toPosition:moveUpstairs()):queryAdd(thing) ~= RETURNVALUE_NOERROR then
                return false
            end

            return thing:teleportTo(toPosition, false)
        elseif thing:isItem() and thing:getType():isMovable() then
            return thing:moveTo(toPosition:moveUpstairs())
        end

        return true
    end

    return false
end

rope:id(2120, 7731)
rope:register()
