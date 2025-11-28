---@type TalkAction
local leaveGuild = TalkAction("!leaveguild")

---@param player Player
---@param words string
---@param param string
function leaveGuild.onSay(player, words, param)
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
    if player:getGuildLevel() == 3 then
        -- Leader level
        player:sendCancelMessage("You cannot leave the guild as a leader. Transfer leadership first or disband the guild.")
        return false
    end

    -- Confirmation check
    if param ~= "yes" then
        player:sendCancelMessage("Are you sure you want to leave your guild? Type '!leaveguild yes' to confirm.")
        return false
    end

    local guildName = guild:getName()

    -- Remove player from guild
    if guild:removeMember(player) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have left the guild '%s'.", guildName))
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_RED)

        -- Notify other guild members
        for _, member in pairs(guild:getMembers()) do
            if member:isOnline() then
                member:sendTextMessage(MESSAGE_INFO_DESCR, string.format("%s has left the guild.", player:getName()))
            end
        end
    else
        player:sendCancelMessage("Failed to leave guild. Please try again.")
        return false
    end

    return true
end

leaveGuild:separator(" ")
leaveGuild:register()
