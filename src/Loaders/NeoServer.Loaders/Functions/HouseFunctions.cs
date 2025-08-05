using LuaNET;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Core.Houses;
using System.Linq;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class HouseFunctions : LuaScriptInterface, IHouseFunctions
{
    private readonly IMap _map;
    private readonly IHouseService _houseService;

    public HouseFunctions(IMap map, IHouseService houseService) : base(nameof(HouseFunctions))
    {
        _map = map;
        _houseService = houseService;
    }

    public void Init(LuaState luaState)
    {
        // Register Game functions for houses
        RegisterMethod(luaState, "Game", "getHouseByPosition", LuaGameGetHouseByPosition);
        RegisterMethod(luaState, "Game", "getHouseByPlayerPosition", LuaGameGetHouseByPlayerPosition);
        RegisterMethod(luaState, "Game", "getHouseById", LuaGameGetHouseById);
        RegisterMethod(luaState, "Game", "purchaseHouse", LuaGamePurchaseHouse);
        RegisterMethod(luaState, "Game", "getHouses", LuaGameGetHouses);
        
        // Register House class methods
        RegisterSharedClass(luaState, "House", "", LuaHouseCreate);
        RegisterMetaMethod(luaState, "House", "__eq", LuaUserdataCompare<House>);
        RegisterMethod(luaState, "House", "getId", LuaHouseGetId);
        RegisterMethod(luaState, "House", "getName", LuaHouseGetName);
        RegisterMethod(luaState, "House", "getOwner", LuaHouseGetOwner);
        RegisterMethod(luaState, "House", "getTownId", LuaHouseGetTownId);
        RegisterMethod(luaState, "House", "getRent", LuaHouseGetRent);
        RegisterMethod(luaState, "House", "getPrice", LuaHouseGetPrice);
        RegisterMethod(luaState, "House", "getSize", LuaHouseGetSize);
        RegisterMethod(luaState, "House", "getEntry", LuaHouseGetEntry);
        RegisterMethod(luaState, "House", "canEnter", LuaHouseCanEnter);
        RegisterMethod(luaState, "House", "canEdit", LuaHouseCanEdit);
        RegisterMethod(luaState, "House", "isPaid", LuaHouseIsPaid);
        RegisterMethod(luaState, "House", "addGuest", LuaHouseAddGuest);
        RegisterMethod(luaState, "House", "removeGuest", LuaHouseRemoveGuest);
        RegisterMethod(luaState, "House", "addSubOwner", LuaHouseAddSubOwner);
        RegisterMethod(luaState, "House", "removeSubOwner", LuaHouseRemoveSubOwner);
        RegisterMethod(luaState, "House", "transferOwnership", LuaHouseTransferOwnership);
        RegisterMethod(luaState, "House", "getGuests", LuaHouseGetGuests);
        RegisterMethod(luaState, "House", "getSubOwners", LuaHouseGetSubOwners);
    }

    private int LuaGameGetHouseByPosition(LuaState luaState)
    {
        // Game.getHouseByPosition(position)
        var position = GetPosition(luaState, 1);
        
        var house = _houseService.GetHouseByPosition(position);
        if (house != null)
        {
            PushHouse(luaState, house);
            return 1;
        }

        Lua.PushNil(luaState);
        return 1;
    }

    private int LuaGameGetHouseByPlayerPosition(LuaState luaState)
    {
        // Game.getHouseByPlayerPosition(position) - searches current position and adjacent positions
        var position = GetPosition(luaState, 1);
        
        // First try the current position
        var house = _houseService.GetHouseByPosition(position);
        if (house != null)
        {
            PushHouse(luaState, house);
            return 1;
        }
        
        // If not found, try adjacent positions (north, south, east, west)
        var adjacentPositions = new[]
        {
            new Location((ushort)(position.X), (ushort)(position.Y - 1), position.Z), // North
            new Location((ushort)(position.X), (ushort)(position.Y + 1), position.Z), // South
            new Location((ushort)(position.X - 1), (ushort)(position.Y), position.Z), // West
            new Location((ushort)(position.X + 1), (ushort)(position.Y), position.Z)  // East
        };
        
        foreach (var adjPosition in adjacentPositions)
        {
            house = _houseService.GetHouseByPosition(adjPosition);
            if (house != null)
            {
                PushHouse(luaState, house);
                return 1;
            }
        }
        
        Lua.PushNil(luaState);
        return 1;
    }

    private int LuaGameGetHouseById(LuaState luaState)
    {
        // Game.getHouseById(houseId)
        var houseId = (uint)Lua.ToNumber(luaState, 1);
        
        var house = _houseService.GetHouseById(houseId);
        if (house != null)
        {
            PushHouse(luaState, house);
            return 1;
        }
        
        Lua.PushNil(luaState);
        return 1;
    }

    private int LuaGamePurchaseHouse(LuaState luaState)
    {
        // Game.purchaseHouse(houseId, playerId)
        var houseId = (uint)Lua.ToNumber(luaState, 1);
        var playerId = (uint)Lua.ToNumber(luaState, 2);
        
        var success = _houseService.PurchaseHouse(houseId, playerId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaGameGetHouses(LuaState luaState)
    {
        // Game.getHouses()
        var houses = _houseService.GetAllHouses();
        
        Lua.NewTable(luaState);
        for (int i = 0; i < houses.Count; i++)
        {
            Lua.PushNumber(luaState, i + 1);
            PushHouse(luaState, houses[i]);
            Lua.SetTable(luaState, -3);
        }
        
        return 1;
    }

    // House object methods
    private int LuaHouseCreate(LuaState luaState)
    {
        // This should not be called directly, houses are created by the game
        Lua.PushNil(luaState);
        return 1;
    }

    private int LuaHouseGetId(LuaState luaState)
    {
        // house:getId()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.Id);
        return 1;
    }

    private int LuaHouseGetName(LuaState luaState)
    {
        // house:getName()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushString(luaState, house.Name);
        return 1;
    }

    private int LuaHouseGetOwner(LuaState luaState)
    {
        // house:getOwner()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.Owner);
        return 1;
    }

    private int LuaHouseGetTownId(LuaState luaState)
    {
        // house:getTownId()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.TownId);
        return 1;
    }

    private int LuaHouseGetRent(LuaState luaState)
    {
        // house:getRent()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.Rent);
        return 1;
    }

    private int LuaHouseGetPrice(LuaState luaState)
    {
        // house:getPrice()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.Price);
        return 1;
    }

    private int LuaHouseGetSize(LuaState luaState)
    {
        // house:getSize()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushNumber(luaState, house.Size);
        return 1;
    }

    private int LuaHouseGetEntry(LuaState luaState)
    {
        // house:getEntry()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        PushPosition(luaState, new Location((ushort)house.Entry.X, (ushort)house.Entry.Y, (byte)house.Entry.Z));
        return 1;
    }

    private int LuaHouseCanEnter(LuaState luaState)
    {
        // house:canEnter(playerId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var playerId = (uint)Lua.ToNumber(luaState, 2);
        Lua.PushBoolean(luaState, house.CanEnter(playerId));
        return 1;
    }

    private int LuaHouseCanEdit(LuaState luaState)
    {
        // house:canEdit(playerId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var playerId = (uint)Lua.ToNumber(luaState, 2);
        Lua.PushBoolean(luaState, house.CanEdit(playerId));
        return 1;
    }

    private int LuaHouseIsPaid(LuaState luaState)
    {
        // house:isPaid()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.PushBoolean(luaState, house.IsPaid());
        return 1;
    }

    private int LuaHouseAddGuest(LuaState luaState)
    {
        // house:addGuest(ownerId, guestId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var ownerId = (uint)Lua.ToNumber(luaState, 2);
        var guestId = (uint)Lua.ToNumber(luaState, 3);
        
        var success = _houseService.AddGuest(house.Id, ownerId, guestId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaHouseRemoveGuest(LuaState luaState)
    {
        // house:removeGuest(ownerId, guestId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var ownerId = (uint)Lua.ToNumber(luaState, 2);
        var guestId = (uint)Lua.ToNumber(luaState, 3);
        
        var success = _houseService.RemoveGuest(house.Id, ownerId, guestId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaHouseAddSubOwner(LuaState luaState)
    {
        // house:addSubOwner(ownerId, subOwnerId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var ownerId = (uint)Lua.ToNumber(luaState, 2);
        var subOwnerId = (uint)Lua.ToNumber(luaState, 3);
        
        var success = _houseService.AddSubOwner(house.Id, ownerId, subOwnerId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaHouseRemoveSubOwner(LuaState luaState)
    {
        // house:removeSubOwner(ownerId, subOwnerId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var ownerId = (uint)Lua.ToNumber(luaState, 2);
        var subOwnerId = (uint)Lua.ToNumber(luaState, 3);
        
        var success = _houseService.RemoveSubOwner(house.Id, ownerId, subOwnerId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaHouseTransferOwnership(LuaState luaState)
    {
        // house:transferOwnership(fromPlayerId, toPlayerId)
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        var fromPlayerId = (uint)Lua.ToNumber(luaState, 2);
        var toPlayerId = (uint)Lua.ToNumber(luaState, 3);
        
        var success = _houseService.TransferHouse(house.Id, fromPlayerId, toPlayerId);
        Lua.PushBoolean(luaState, success);
        return 1;
    }

    private int LuaHouseGetGuests(LuaState luaState)
    {
        // house:getGuests()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.NewTable(luaState);
        for (int i = 0; i < house.Guests.Count; i++)
        {
            Lua.PushNumber(luaState, i + 1);
            Lua.PushNumber(luaState, house.Guests[i]);
            Lua.SetTable(luaState, -3);
        }
        
        return 1;
    }

    private int LuaHouseGetSubOwners(LuaState luaState)
    {
        // house:getSubOwners()
        var house = GetUserdata<House>(luaState, 1);
        if (house == null)
        {
            ReportError(luaState, "House not found");
            return 0;
        }

        Lua.NewTable(luaState);
        for (int i = 0; i < house.SubOwners.Count; i++)
        {
            Lua.PushNumber(luaState, i + 1);
            Lua.PushNumber(luaState, house.SubOwners[i]);
            Lua.SetTable(luaState, -3);
        }
        
        return 1;
    }

    private void PushHouse(LuaState luaState, House house)
    {
        // Push the house as a userdata with House metatable
        PushUserdata(luaState, house);
        SetMetatable(luaState, -1, "House");
    }
}
