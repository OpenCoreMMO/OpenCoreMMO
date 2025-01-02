function Creature.getClosestFreePosition(self, position, maxRadius, mustBeReachable)
	maxRadius = maxRadius or 1

	-- backward compatability (extended)
	if maxRadius == true then
		maxRadius = 2
	end

	local checkPosition = Position(position)
	local closestDistance = -1
	local closestPosition = Position()
	for radius = 0, maxRadius do
		checkPosition.x = checkPosition.x - math.min(1, radius)
		checkPosition.y = checkPosition.y + math.min(1, radius)
		if closestDistance ~= -1 then
			return closestPosition
		end

		local total = math.max(1, radius * 8)
		for i = 1, total do
			if radius > 0 then
				local direction = math.floor((i - 1) / (radius * 2))
				checkPosition:getNextPosition(direction)
			end

			local tile = Tile(checkPosition)
			if tile and tile:getCreatureCount() == 0 and not tile:hasProperty(CONST_PROP_IMMOVABLEBLOCKSOLID) and (not mustBeReachable or self:getPathTo(checkPosition)) then
				local distance = self:getPosition():getDistance(checkPosition)
				if closestDistance == -1 or closestDistance > distance then
					closestDistance = distance
					closestPosition = Position(checkPosition)
				end
			end
		end
	end
	return closestPosition
end

function Creature.getMonster(self)
    return self:isMonster() and self or nil
end

function Creature.getPlayer(self)
    return self:isPlayer() and self or nil
end

function Creature.isContainer(self)
    return false
end

function Creature.isItem(self)
    return false
end

function Creature.isMonster(self)
    return false
end

function Creature.isNpc(self)
    return false
end

function Creature.isPlayer(self)
    return false
end

function Creature.isTeleport(self)
    return false
end

function Creature.isTile(self)
    return false
end
