using LuaNET;
using NeoServer.Domain.Creatures.Player.Vocation;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class VocationFunctions : LuaScriptInterface, IVocationFunctions
{
    private static IVocationStore _vocationStore;

    public VocationFunctions(IVocationStore vocationStore) : base(nameof(VocationFunctions))
    {
        _vocationStore = vocationStore;
    }
    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Vocation", "", LuaCreateVocation);
        RegisterMetaMethod(luaState, "Vocation", "__eq", LuaUserdataCompare<Vocation>);

        RegisterMethod(luaState, "Vocation", "getBaseId", LuaVocationGetBaseId);
    }

    public static int LuaCreateVocation(LuaState luaState)
    {
        // Vocation(id or name)
        Vocation vocation;
        if (IsNumber(luaState, 2))
        {
            var vocationId = GetNumber<uint>(luaState, 2);
            _vocationStore.TryGetValue((byte)vocationId, out vocation);
        }
        else
        {
            var vocationName = GetString(luaState, 2);
            vocation = _vocationStore.GetByName(vocationName);
        }

        if (vocation != null)
        {
            PushUserdata(luaState, vocation);
            SetMetatable(luaState, -1, "Vocation");
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }

    private static int LuaVocationGetBaseId(LuaState luaState)
    {
        // vocation:getBaseId()
        var vocation = GetUserdata<Vocation>(luaState, 1);
        if (vocation != null)
        {
            Lua.PushNumber(luaState, vocation.BaseId);
        }
        else
        {
            Lua.PushNil(luaState);
        }
        return 1;
    }
}