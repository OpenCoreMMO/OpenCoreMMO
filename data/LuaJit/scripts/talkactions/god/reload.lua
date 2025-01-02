local talkAction = TalkAction("/reload")

local reloadTypes = {
        ["all"] = RELOAD_TYPE_ALL,

        ["chat"] = RELOAD_TYPE_CHAT,
        ["channel"] = RELOAD_TYPE_CHAT,
        ["chatchannels"] = RELOAD_TYPE_CHAT,
        ["config"] = RELOAD_TYPE_CONFIG,
        ["configuration"] = RELOAD_TYPE_CONFIG,

        ["events"] = RELOAD_TYPE_EVENTS,

        ["items"] = RELOAD_TYPE_ITEMS,
        ["module"] = RELOAD_TYPE_MODULES,
        ["modules"] = RELOAD_TYPE_MODULES,

        ["monster"] = RELOAD_TYPE_MONSTERS,
        ["monsters"] = RELOAD_TYPE_MONSTERS,

        ["mount"] = RELOAD_TYPE_MOUNTS,
        ["mounts"] = RELOAD_TYPE_MOUNTS,

        ["npc"] = RELOAD_TYPE_NPCS,
        ["npcs"] = RELOAD_TYPE_NPCS,

        ["raid"] = RELOAD_TYPE_RAIDS,
        ["raids"] = RELOAD_TYPE_RAIDS,

        ["scripts"] = RELOAD_TYPE_SCRIPTS,
        ["script"] = RELOAD_TYPE_SCRIPTS,

        ["rate"] = RELOAD_TYPE_CORE,
        ["rates"] = RELOAD_TYPE_CORE,
        ["stage"] = RELOAD_TYPE_CORE,
        ["stages"] = RELOAD_TYPE_CORE,
        ["global"] = RELOAD_TYPE_CORE,
        ["core"] = RELOAD_TYPE_CORE,
        ["lib"] = RELOAD_TYPE_CORE,
        ["libs"] = RELOAD_TYPE_CORE,

        ["imbuements"] = RELOAD_TYPE_IMBUEMENTS,

        ["group"] = RELOAD_TYPE_GROUPS,
        ["groups"] = RELOAD_TYPE_GROUPS,
    }

function talkAction.onSay(player, words, param)
    if not player:getGroup():getAccess() then
		return true
	end

    --if not configManager.getBoolean(configKeys.ALLOW_RELOAD) then
    --	print("Reload command is disabled.")
    --	player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Reload command is disabled.")
    --	return true
    --end

    if param == "" then
        logger.warn("Command param required.")
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Command param required.")
        return true
    end

    -- create log
    -- logCommand(self, "/reload", param)

    local reloadType = reloadTypes[param:lower()]
    if reloadType then
        -- Force save server before reload
        --nsaveServer()
        --bSaveHirelings()
        local reloadingMessage = string.format("Server is saved.. Now will reload %s!", param:lower())
        logger.info(reloadingMessage)
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, reloadingMessage)

        local reloadedMessage = string.format("Reloaded %s.", param:lower())

        Game.reload(reloadType)
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, reloadedMessage)
        logger.info(reloadedMessage)

    elseif not reloadType then
        player:sendTextMessage(MESSAGE_STATUS_CONSOLE_BLUE, "Reload type not found.")
        logger.warn("[reload.onSay] - Reload type '{}' not found", param)
    end

    return true
end

talkAction:separator(" ")
talkAction:register()
