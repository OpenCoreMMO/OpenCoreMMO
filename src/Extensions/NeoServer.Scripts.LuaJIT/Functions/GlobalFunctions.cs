using LuaNET;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GlobalFunctions : LuaScriptInterface, IGlobalFunctions
{
    public GlobalFunctions(
        ILuaEnvironment luaEnvironment, 
        ILogger logger) : base(nameof(GlobalFunctions))
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
