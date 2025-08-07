using LuaNET;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.World;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class TownFunctions : LuaScriptInterface, ITownFunctions
{
    private static World _world;

    public TownFunctions(World world) : base(nameof(TownFunctions))
    {
        _world = world;
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Town", "", LuaCreateTown);
        RegisterMetaMethod(luaState, "Town", "__eq", LuaUserdataCompare<ITown>);

        RegisterMethod(luaState, "Town", "getId", LuaTownGetId);
        RegisterMethod(luaState, "Town", "getName", LuaTownGetName);
        RegisterMethod(luaState, "Town", "getTemplePosition", LuaTownGetTemplePosition);
    }

    public static int LuaCreateTown(LuaState luaState)
    {
        // Town(id or name)
        ITown town = null;

        if (IsNumber(luaState, 2))
        {
            var id = GetNumber<uint>(luaState, 2);
            _world.TryGetTown(id, out town);
        }
        else if (IsString(luaState, 2))
        {
            var name = GetString(luaState, 2);
            _world.TryGetTown(name, out town);
        }

        if (town != null)
        {
            PushUserdata(luaState, town);
            SetMetatable(luaState, -1, "Town");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }

    public static int LuaTownGetId(LuaState luaState)
    {
        // Town:getId()
        var town = GetUserdata<ITown>(luaState, 1);
        if (town != null)
            Lua.PushNumber(luaState, town.Id);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    public static int LuaTownGetName(LuaState luaState)
    {
        // Town:getName()
        var town = GetUserdata<ITown>(luaState, 1);
        if (town != null)
            Lua.PushString(luaState, town.Name);
        else
            Lua.PushNil(luaState);
        return 1;
    }

    public static int LuaTownGetTemplePosition(LuaState luaState)
    {
        // Town:getTemplePosition()
        var town = GetUserdata<ITown>(luaState, 1);
        if (town != null)
            PushPosition(luaState, town.Coordinate.Location);
        else
            Lua.PushNil(luaState);
        return 1;
    }
}