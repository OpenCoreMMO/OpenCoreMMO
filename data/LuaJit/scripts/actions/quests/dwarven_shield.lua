local dwarvenShieldQuest = Action()

function dwarvenShieldQuest.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    logger.info("dwarvenShield.onUse")

    local questStorage = player:getStorageValue(Storage.Quest.DwarvenShieldQuest.Key)
	if questStorage > -1 then
		player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "It is empty.")
        return true
	end

    local dwarvenShieldId = 2490

    local itemType = ItemType(dwarvenShieldId)
	if itemType:getId() == 0 then
		return false
	end

	local itemWeight = itemType:getWeight()
	local playerCap = player:getFreeCapacity()

    if playerCap >= itemWeight then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. '.')
        player:addItem(dwarvenShieldId, 1)
        player:setStorageValue(Storage.Quest.DwarvenShieldQuest.Key, 1)
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. ' weighing ' .. itemWeight .. ' oz it\'s too heavy.')
    end

    return true
end

dwarvenShieldQuest:uid(60051)
dwarvenShieldQuest:register()
