using LuaNET;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class GlobalLuaMapping : LuaScriptInterface, IGlobalLuaMapping
{
    public GlobalLuaMapping() : base(nameof(GlobalLuaMapping))
    {
    }

    public void Init(LuaState L)
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
