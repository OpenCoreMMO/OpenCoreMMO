using LuaNET;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ShopFunctions : LuaScriptInterface, IShopFunctions
{
    public ShopFunctions() : base(nameof(ShopFunctions))
    {
    }

    public void Init(LuaState luaState)
    {
        RegisterSharedClass(luaState, "Shop", "", LuaShopCreate);
    }

    private static int LuaShopCreate(LuaState luaState)
    {
        // Shop(uid)
        var id = GetNumber<uint>(luaState, 2);

        var item = GetScriptEnv().GetItemByUID(id);
        if (item != null)
        {
            PushUserdata(luaState, item);
            SetMetatable(luaState, -1, "Shop");
        }
        else
        {
            Lua.PushNil(luaState);
        }

        return 1;
    }
}