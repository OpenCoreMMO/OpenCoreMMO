function doRelocate(fromPos, toPos)
    if fromPos == toPos then
        return false
    end

    local fromTile = Tile(fromPos)
    if not fromTile or not Tile(toPos) then
        return false
    end

    return fromTile:relocateTo(toPos)
end
