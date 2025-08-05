using System;
using LuaNET;
using NeoServer.Domain.Core.Houses;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.Houses;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class HouseUserdata : LuaScriptInterface, IHouseUserdata
{
    public HouseUserdata() : base(nameof(HouseUserdata))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "House", "", LuaHouseCreate);
        LoadFunctions(luaState);
    }

    private static int LuaHouseCreate(LuaState luaState)
    {
        // This is a placeholder - houses should not be created from Lua
        Lua.PushNil(luaState);
        return 1;
    }

    public void LoadFunctions(LuaState luaState)
    {
        // Register basic House userdata methods for !buyhouse
        RegisterMethod(luaState, "House", "getId", LuaHouseGetId);
        RegisterMethod(luaState, "House", "getOwner", LuaHouseGetOwner);
        RegisterMethod(luaState, "House", "setOwner", LuaHouseSetOwner);
        RegisterMethod(luaState, "House", "getPrice", LuaHouseGetPrice);
        RegisterMethod(luaState, "House", "getRent", LuaHouseGetRent);
        RegisterMethod(luaState, "House", "getEntry", LuaHouseGetEntry);
        RegisterMethod(luaState, "House", "setPaidUntil", LuaHouseSetPaidUntil);
        RegisterMethod(luaState, "House", "getPaidUntil", LuaHouseGetPaidUntil);
        RegisterMethod(luaState, "House", "getDoorByPosition", LuaHouseGetDoorByPosition);
    }

    private static int LuaHouseGetId(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, house.Id);
        return 1;
    }

    private static int LuaHouseGetOwner(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, house.Owner);
        return 1;
    }

    private static int LuaHouseSetOwner(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var playerId = (uint)Lua.ToNumber(luaState, 2);
        
        // Log the ownership change
        Console.WriteLine($"[HouseUserdata] Setting house {house.Id} owner to {playerId}");
        
        // Get the old owner for comparison
        var oldOwner = house.Owner;
        
        // Set the owner in memory
        house.Owner = playerId;
        
        // If setting owner to 0 (leaving house), reset paid until as well
        if (playerId == 0)
        {
            house.PaidUntil = 0;
            Console.WriteLine($"[HouseUserdata] House {house.Id} ownership removed, PaidUntil reset");
        }
        else
        {
            // Set paid until 30 days from now when buying a house
            var paidUntil = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();
            house.PaidUntil = paidUntil;
            Console.WriteLine($"[HouseUserdata] House {house.Id} ownership set, PaidUntil set to {paidUntil}");
        }
        
        // Persist the ownership change to database/file
        try
        {
            // Use HouseService's static persistence method
            if (playerId == 0)
            {
                // Leaving house - save with owner 0
                HouseService.SaveHouseOwnershipStatic(house.Id, 0, 0);
                Console.WriteLine($"[HouseUserdata] Called static save for leaving house {house.Id}");
            }
            else
            {
                // Buying house - save with new owner and paid until date
                HouseService.SaveHouseOwnershipStatic(house.Id, playerId, house.PaidUntil);
                Console.WriteLine($"[HouseUserdata] Called static save for purchasing house {house.Id}, new owner {playerId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HouseUserdata] Error persisting house ownership change: {ex.Message}");
            // Don't fail the operation, just log the error
        }
        
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaHouseGetPrice(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, house.Price);
        return 1;
    }

    private static int LuaHouseGetRent(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, house.Rent);
        return 1;
    }

    private static int LuaHouseGetEntry(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        // Push the entry position as a Position userdata
        var entryLocation = new Location((ushort)house.Entry.X, (ushort)house.Entry.Y, (byte)house.Entry.Z);
        LuaFunctionsLoader.PushPosition(luaState, entryLocation);
        return 1;
    }

    private static int LuaHouseSetPaidUntil(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var timestamp = (long)Lua.ToNumber(luaState, 2);
        house.PaidUntil = timestamp;
        
        Lua.PushBoolean(luaState, true);
        return 1;
    }

    private static int LuaHouseGetPaidUntil(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Lua.PushNumber(luaState, 0);
            return 1;
        }

        Lua.PushNumber(luaState, house.PaidUntil);
        return 1;
    }

    private static int LuaHouseGetDoorByPosition(LuaState luaState)
    {
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            Console.WriteLine("[HouseUserdata] getDoorByPosition: House is null");
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        // Get position from Lua
        var position = LuaFunctionsLoader.GetPosition(luaState, 2);
        if (position == null)
        {
            Console.WriteLine("[HouseUserdata] getDoorByPosition: Invalid position");
            Lua.PushBoolean(luaState, false);
            return 1;
        }

        var coordinate = new Coordinate((ushort)position.X, (ushort)position.Y, (sbyte)position.Z);
        bool isDoor = house.GetDoorByPosition(coordinate);
        
        Console.WriteLine($"[HouseUserdata] getDoorByPosition: House {house.Id}, position {coordinate.X},{coordinate.Y},{coordinate.Z}, isDoor: {isDoor}");
        
        Lua.PushBoolean(luaState, isDoor);
        return 1;
    }
}