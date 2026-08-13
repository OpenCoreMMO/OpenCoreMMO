using LuaNET;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.Services;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

/// <summary>
///     Minimal House Lua bindings for Phase 3 spell slices (aleta sio, aleta som, alana sio, aleta grav).
///     Full HouseFunctions surface (tiles, beds, rent, trade, save) comes in later slices.
/// </summary>
public class HouseFunctions : LuaScriptInterface, IHouseFunctions
{
    private static IHouseStore _houseStore;
    private static IHouseService _houseService;

    public HouseFunctions(IHouseStore houseStore, IHouseService houseService) :
        base(nameof(HouseFunctions))
    {
        _houseStore = houseStore;
        _houseService = houseService;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "House", "", LuaHouseCreate);
        RegisterMetaMethod(luaState, "House", "__eq", LuaUserdataCompare<House>);

        RegisterMethod(luaState, "House", "getId", LuaHouseGetId);
        RegisterMethod(luaState, "House", "canEditAccessList", LuaHouseCanEditAccessList);
        RegisterMethod(luaState, "House", "getAccessList", LuaHouseGetAccessList);
        RegisterMethod(luaState, "House", "getDoorIdByPosition", LuaHouseGetDoorIdByPosition);
        RegisterMethod(luaState, "House", "kickPlayer", LuaHouseKickPlayer);

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

    private static int LuaHouseGetDoorIdByPosition(LuaState luaState)
    {
        // house:getDoorIdByPosition(position)
        var house = GetUserdata<House>(luaState, 1);
        if (house is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        var position = GetPosition(luaState, 2);
        var doorId = house.GetDoorIdByPosition(position);
        if (doorId is null)
        {
            Lua.PushNil(luaState);
            return 1;
        }

        Lua.PushNumber(luaState, doorId.Value);
        return 1;
    }

    private static int LuaHouseKickPlayer(LuaState luaState)
    {
        // house:kickPlayer(caster, target) -> bool
        var house = GetUserdata<House>(luaState, 1);
        var caster = GetUserdata<IPlayer>(luaState, 2);
        var target = GetUserdata<IPlayer>(luaState, 3);

        if (house is null || caster is null || target is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        // HouseService validates CanKick (relative access, CanEditHouses, tile).
        PushBoolean(luaState, _houseService.KickPlayer(house, caster, target));
        return 1;
    }
}
