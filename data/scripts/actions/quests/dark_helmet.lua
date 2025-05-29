local darkHelmetQuest = Action()

function darkHelmetQuest.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local questStorage = player:getStorageValue(Storage.Quest.DarkHelmetQuest.Key)
	if questStorage > -1 then
		player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "It is empty.")
        return true
	end

    local darkHelmetId = 2525

    local itemType = ItemType(darkHelmetId)
	if itemType:getId() == 0 then
		return false
	end

	local itemWeight = itemType:getWeight()
	local playerCap = player:getFreeCapacity()

    if playerCap >= itemWeight then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. '.')
        player:addItem(darkHelmetId, 1)
        player:setStorageValue(Storage.Quest.DarkHelmetQuest.Key, 1)
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. ' weighing ' .. itemWeight .. ' oz it\'s too heavy.')
    end

    return true
end

darkHelmetQuest:uid(60052)
darkHelmetQuest:register()
