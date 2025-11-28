---@type TalkAction
local joinGuild = TalkAction("!joinguild")

---@param player Player
---@param words string
---@param param string
function joinGuild.onSay(player, words, param)
    if not player then
        return false
    end

    -- Check if player is already in a guild
    if player:getGuild() then
        player:sendCancelMessage("You are already in a guild.")
        return false
    end

    -- Check if player has a guild invitation
    if param == "" then
        player:sendCancelMessage("Please enter the guild name you want to join.")
        return false
    end

    -- Find the guild by name
    local guild = Guild(param)
    if not guild then
        player:sendCancelMessage("Guild not found.")
        return false
    end

    -- Check if player has an invitation to this guild
    if not guild:hasInvitation(player) then
        player:sendCancelMessage("You don't have an invitation to this guild.")
        return false
    end

    -- Add player to guild
    if guild:addMember(player) then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, string.format("You have joined the guild '%s'!", param))
        player:getPosition():sendMagicEffect(CONST_ME_MAGIC_BLUE)

        -- Notify other guild members
        for _, member in pairs(guild:getMembers()) do
            if member:isOnline() and member ~= player then
                member:sendTextMessage(MESSAGE_INFO_DESCR, string.format("%s has joined the guild.", player:getName()))
            end
        end
    else
        player:sendCancelMessage("Failed to join guild. Please try again.")
        return false
    end

    return true
end

joinGuild:separator(" ")
joinGuild:register()
