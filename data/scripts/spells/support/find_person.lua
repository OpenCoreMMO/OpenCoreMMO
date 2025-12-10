local spell = Spell("instant")

function spell.onCastSpell(creature, variant)
    local caster = Player(creature)
    if not caster then
        return false
    end

    local targetName = variant:getString()
    if not targetName or targetName == "" then
        caster:sendTextMessage(MESSAGE_STATUS_SMALL, "You need to specify a player.")
        return false
    end

    local target = Player(targetName)
    if not target then
        caster:sendTextMessage(MESSAGE_STATUS_SMALL, "A player with this name is not online.")
        return false
    end

    local fromPos = caster:getPosition()
    local toPos = target:getPosition()

    -- floor check first
    if fromPos.z ~= toPos.z then
        local dirText = fromPos.z > toPos.z and "above you" or "below you"
        caster:sendTextMessage(MESSAGE_STATUS_SMALL, "You sense " .. target:getName() .. " is " .. dirText .. ".")
        fromPos:sendMagicEffect(CONST_ME_MAGIC_BLUE)
        return true
    end

    local dx = toPos.x - fromPos.x
    local dy = toPos.y - fromPos.y

    local absDx, absDy = math.abs(dx), math.abs(dy)
    local dist = math.max(absDx, absDy)

    local dirY = ""
    if dy < 0 then
        dirY = "north"
    elseif dy > 0 then
        dirY = "south"
    end

    local dirX = ""
    if dx > 0 then
        dirX = "east"
    elseif dx < 0 then
        dirX = "west"
    end

    local dir
    if dirY ~= "" and dirX ~= "" then
        dir = dirY .. "-" .. dirX
    elseif dirY ~= "" then
        dir = dirY
    elseif dirX ~= "" then
        dir = dirX
    else
        dir = "here"
    end

    local distanceText
    if dist <= 1 then
        distanceText = "is standing next to you"
    elseif dist <= 4 then
        distanceText = "is very close to you"
    elseif dist <= 30 then
        distanceText = "is to the " .. dir
    else
        distanceText = "is far to the " .. dir
    end

    caster:sendTextMessage(MESSAGE_STATUS_SMALL, "You sense " .. target:getName() .. " " .. distanceText .. ".")
    fromPos:sendMagicEffect(CONST_ME_MAGIC_BLUE)
    return true
end

spell:name("Find Person")
spell:words("exiva")
spell:group("support")
spell:vocation("sorcerer;true", "master sorcerer;true", "druid;true", "elder druid;true")
spell:id(53)
spell:cooldown(1 * 1000)
spell:groupCooldown(1 * 1000)
spell:level(8)
spell:mana(20)
spell:hasParams(true)
spell:isAggressive(false)
spell:needLearn(false)
spell:isPremium(false)
spell:register()
