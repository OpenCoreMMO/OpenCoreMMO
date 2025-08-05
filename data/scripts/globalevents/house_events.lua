-- Event to load houses on server startup
local houseLoader = GlobalEvent("HouseLoader")

function houseLoader.onStartup()
    print("Loading houses from database...")
    
    -- This would typically be called from the C# service layer during startup
    -- The actual implementation would be in HouseService.LoadAllHousesAsync()
    
    local success = Game.loadHouses()
    if success then
        local houses = Game.getHouses()
        print(string.format("Successfully loaded %d houses.", #houses))
        
        -- Log house statistics
        local owned = 0
        local available = 0
        local expired = 0
        
        for _, house in ipairs(houses) do
            local owner = house:getOwner()
            if owner == 0 then
                available = available + 1
            elseif house:isPaid() then
                owned = owned + 1
            else
                expired = expired + 1
            end
        end
        
        print(string.format("House status: %d owned, %d available, %d expired", owned, available, expired))
    else
        print("ERROR: Failed to load houses from database!")
    end
    
    return true
end

houseLoader:register()

-- Event to save houses periodically (every 5 minutes)
local houseSaver = GlobalEvent("HouseSaver")

function houseSaver.onThink(interval)
    print("Auto-saving houses...")
    
    local success = Game.saveHouses()
    if success then
        print("Houses saved successfully.")
    else
        print("ERROR: Failed to save houses!")
    end
    
    return true
end

houseSaver:interval(300000) -- 5 minutes in milliseconds
houseSaver:register()

print("House events loaded successfully!")
