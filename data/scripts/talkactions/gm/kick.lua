---@type TalkAction
local talkAction = TalkAction("/kick")

---@param player Player
---@param words string
---@param param string
function talkAction.onSay(player, words, param)
    -- Check if player has GM access
    if player:getGroup():getAccess() == 0 then
        return true
    end

    -- Check if parameters are provided
    if param == "" then
        player:sendCancelMessage("Usage: /kick <PlayerName> [reason]")
        return true
    end

    -- Parse parameters - support both "Name reason" and "Name, reason" formats
    local targetName, reason
    local commaPos = param:find(",")
    
    if commaPos then
        -- Format: "Name, reason"
        targetName = param:sub(1, commaPos - 1):trim()
        reason = param:sub(commaPos + 1):trim()
    else
        -- Format: "Name reason" - split on first space
        local spacePos = param:find(" ")
        if spacePos then
            targetName = param:sub(1, spacePos - 1):trim()
            reason = param:sub(spacePos + 1):trim()
        else
            -- Just the name, no reason
            targetName = param:trim()
            reason = nil
        end
    end

    -- Find the target player
    local target = Player(targetName)
    if not target then
        player:sendCancelMessage("Player '" .. targetName .. "' is not online.")
        return true
    end

    -- Prevent self-kick
    if target:getId() == player:getId() then
        player:sendCancelMessage("You cannot kick yourself.")
        return true
    end

    -- Prevent kicking GMs with equal or greater access
    local playerGroup = player:getGroup()
    local targetGroup = target:getGroup()
    
    if targetGroup:getAccess() > 0 and targetGroup:getId() >= playerGroup:getId() then
        player:sendCancelMessage("You cannot kick a GM with equal or greater access level.")
        return true
    end

    -- Prepare kick message and log entry
    local kickMessage
    local logMessage
    
    if reason and reason ~= "" then
        kickMessage = "You have been kicked by " .. player:getName() .. ". Reason: " .. reason
        logMessage = "[KICK] " .. player:getName() .. " kicked " .. target:getName() .. " (reason: " .. reason .. ")"
    else
        kickMessage = "You have been kicked by " .. player:getName() .. "."
        logMessage = "[KICK] " .. player:getName() .. " kicked " .. target:getName()
    end

    -- Log the action
    logger.info(logMessage)

    -- Notify the target player
    target:sendTextMessage(MESSAGE_STATUS_WARNING, kickMessage)

    -- Notify the GM
    player:sendTextMessage(MESSAGE_INFO_DESCR, target:getName() .. " has been kicked.")

    -- Disconnect the target player after a short delay
    addEvent(function()
        if target then
            target:remove()
        end
    end, 1000)

    return true
end

talkAction:register()