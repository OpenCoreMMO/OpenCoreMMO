---@type TalkAction
local disbandGuild = TalkAction("!disbandguild")

---@param player Player
---@param words string
---@param param string
function disbandGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is in a guild
    local guild = player:getGuild()
    if not guild then
        player:sendCancelMessage("You are not in a guild.")
        return false
    end

    -- Check if player is the guild leader
    if player:getGuildLevel() ~= 3 then
        -- Only leaders can disband
        player:sendCancelMessage("Only the guild leader can disband the guild.")
        return false
    end

    -- Confirmation check
    if param ~= "yes" then
        player:sendCancelMessage("Are you sure you want to disband your guild? This action cannot be undone. Type '!disbandguild yes' to confirm.")
        return false
    end

    local guildName = guild:getName()

    -- Notify all online guild members first
    for _, member in pairs(guild:getMembers()) do
        if member:isOnline() then
            if member ~= player then
                member:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("The guild '%s' has been disbanded by %s.", guildName, player:getName()))
                member:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
            end
        end
    end

    -- Disband the guild
    if guild:disband() then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have disbanded the guild '%s'. All members have been removed.", guildName))
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)
    else
        player:sendCancelMessage("Failed to disband guild. Please try again.")
        return false
    end

    return true
end

disbandGuild:separator(" ")
disbandGuild:register()
