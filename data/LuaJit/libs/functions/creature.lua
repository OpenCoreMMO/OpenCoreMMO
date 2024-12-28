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
