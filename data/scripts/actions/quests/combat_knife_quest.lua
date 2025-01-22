local combatKnifeQuest = Action()

function combatKnifeQuest.onUse(player, item, fromPosition, target, toPosition, isHotkey)
    local questStorage = player:getStorageValue(Storage.Quest.CombatKnifeQuest.Key)
	if questStorage > -1 then
		player:sendTextMessage(MESSAGE_EVENT_ADVANCE, "It is empty.")
        return true
	end

    local combatKnifeId = 2404

    local itemType = ItemType(combatKnifeId)
	if itemType:getId() == 0 then
		return false
	end

	local itemWeight = itemType:getWeight()
	local playerCap = player:getFreeCapacity()

    if playerCap >= itemWeight then
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. '.')
        player:addItem(combatKnifeId, 1)
        player:setStorageValue(Storage.Quest.CombatKnifeQuest.Key, 1)
    else
        player:sendTextMessage(MESSAGE_EVENT_ADVANCE, 'You have found a ' .. itemType:getName() .. ' weighing ' .. itemWeight .. ' oz it\'s too heavy.')
    end

    return true
end

combatKnifeQuest:uid(60047)
combatKnifeQuest:register()
