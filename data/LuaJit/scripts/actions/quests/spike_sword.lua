local spikeSwordQuest = Action()

function spikeSwordQuest.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local questStorage = player:getStorageValue(Storage.Quest.SpikeSwordQuest.Key)
	if questStorage > -1 then
		player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "It is empty.")
        return true
	end

    local spikeSwordId = 2383

    local itemType = ItemType(spikeSwordId)
	if itemType:getId() == 0 then
		return false
	end

	local itemWeight = itemType:getWeight()
	local playerCap = player:getFreeCapacity()

    if playerCap >= itemWeight then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. '.')
        player:addItem(spikeSwordId, 1)
        player:setStorageValue(Storage.Quest.SpikeSwordQuest.Key, 1)
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. ' weighing ' .. itemWeight .. ' oz it\'s too heavy.')
    end

    return true
end

spikeSwordQuest:uid(60050)
spikeSwordQuest:register()
