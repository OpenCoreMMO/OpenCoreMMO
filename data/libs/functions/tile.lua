function Tile.isCreature(self)
    return false
end

function Tile.isItem(self)
    return false
end

function Tile.isTile(self)
    return true
end

function Tile.isContainer(self)
    return false
end

function Tile.relocateTo(self, toPosition)
    if self:getPosition() == toPosition or not Tile(toPosition) then
        return false
    end

    --todo: implements ThingFunctions
    for i = self:getThingCount() - 1, 0, -1 do
        local thing = self:getThing(i)
        if thing then
            if thing:isItem() then
                if thing:getFluidType() ~= 0 then
                    thing:remove()
                elseif ItemType(thing:getId()):isMovable() then
                    thing:moveTo(toPosition)
                end
            elseif thing:isCreature() then
                thing:teleportTo(toPosition)
            end
        end
    end

    return true
end

function Tile.isWalkable(self)
    local ground = self:getGround()
    if not ground or ground:hasProperty(CONST_PROP_BLOCKSOLID) then
        return false
    end

    local items = self:getItems()
    for i = 1, self:getItemCount() do
        local item = items[i]
        local itemType = item:getType()
        if itemType:getType() ~= ITEM_TYPE_MAGICFIELD and not itemType:isMovable() and item:hasProperty(CONST_PROP_BLOCKSOLID) then
            return false
        end
    end
    return true
end

-- Functions from OTServbr-Global
--todo: Implements HouseFunctions
function Tile.isHouse(self)
	local house = self:getHouse()
	return house and true or false
end

function Tile.isPz(self)
	return self:hasFlag(TILESTATE_PROTECTIONZONE)
end

function Tile:isRopeSpot()
	if not self then
		return false
	end

	if not self:getGround() then
		return false
	end

	if table.contains(ropeSpots, self:getGround():getId()) then
		return true
	end

	for i = 1, self:getTopItemCount() do
		local thing = self:getThing(i)
        --todo: implements specialRopeSpots
		if thing and table.contains(specialRopeSpots, thing:getId()) then
			return true
		end
	end

	return false
end