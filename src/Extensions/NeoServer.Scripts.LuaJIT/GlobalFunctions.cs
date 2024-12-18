using LuaNET;

namespace NeoServer.Scripts.LuaJIT;

public class GlobalFunctions : LuaScriptInterface
{
    public GlobalFunctions() : base(nameof(GlobalFunctions))
    {
    }

    public static void Init(LuaState L)
    {
        RegisterGlobalMethod(L, "rawgetmetatable", LuaRawGetMetatable);
    }

    private static int LuaRawGetMetatable(LuaState L)
    {
        // rawgetmetatable(metatableName)
        Lua.GetMetaTable(L, GetString(L, 1));
        return 1;
    }
}
