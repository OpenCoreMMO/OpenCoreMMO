using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Houses;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

/// <summary>
///     Minimal House Lua bindings for Phase 3 spell slice (aleta sio).
///     Full HouseFunctions surface (doors, trade, rent, etc.) comes in later slices.
/// </summary>
public class HouseFunctions : LuaScriptInterface, IHouseFunctions
{
    private static IHouseStore _houseStore;

    public HouseFunctions(IHouseStore houseStore) : base(nameof(HouseFunctions))
    {
        _houseStore = houseStore;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "House", "", LuaHouseCreate);
        RegisterMetaMethod(luaState, "House", "__eq", LuaUserdataCompare<House>);

        RegisterMethod(luaState, "House", "getId", LuaHouseGetId);
        RegisterMethod(luaState, "House", "canEditAccessList", LuaHouseCanEditAccessList);
        RegisterMethod(luaState, "House", "getAccessList", LuaHouseGetAccessList);

        RegisterGlobalVariable(luaState, "GUEST_LIST", HouseListId.GuestList);
        RegisterGlobalVariable(luaState, "SUBOWNER_LIST", HouseListId.SubOwnerList);
    }

    private static int LuaHouseCreate(LuaState luaState)
    {
        // House(id)
        var houseId = GetNumber<uint>(luaState, 2);
        var house = _houseStore.GetByHouseId(houseId);
        if (house is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        PushUserdata(luaState, house);
        SetMetatable(luaState, -1, "House");
        return 1;
    }

    private static int LuaHouseGetId(LuaState luaState)
    {
        // house:getId()
        var house = GetUserdata<House>(luaState, 1);
        if (house is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushNumber(luaState, house.Id);
        return 1;
    }

    private static int LuaHouseCanEditAccessList(LuaState luaState)
    {
        // house:canEditAccessList(listId, player)
        var house = GetUserdata<House>(luaState, 1);
        var listId = GetNumber<uint>(luaState, 2);
        var player = GetUserdata<IPlayer>(luaState, 3);

        if (house is null || player is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        PushBoolean(luaState, house.CanEditAccessList(listId, player));
        return 1;
    }

    private static int LuaHouseGetAccessList(LuaState luaState)
    {
        // house:getAccessList(listId)
        var house = GetUserdata<House>(luaState, 1);
        var listId = GetNumber<uint>(luaState, 2);

        if (house is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var list = house.GetAccessList(listId);
        Lua.PushString(luaState, list?.RawText ?? string.Empty);
        return 1;
    }
}
