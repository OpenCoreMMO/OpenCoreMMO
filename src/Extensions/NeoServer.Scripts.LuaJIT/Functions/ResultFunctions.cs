using LuaNET;
using NeoServer.Data.Contexts;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public partial class ResultFunctions : LuaScriptInterface, IResultFunctions
{
    private static ILogger _logger;
    private static NeoContext _dbContext;

    public ResultFunctions(
        ILogger logger,
        NeoContext dbContext) : base(nameof(ResultFunctions))

    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public void Init(LuaState luaState)
    {
        RegisterTable(luaState, "Result");
        RegisterMethod(luaState, "Result", "getNumber", LuaResultGetNumber);
        RegisterMethod(luaState, "Result", "getString", LuaResultGetString);
        RegisterMethod(luaState, "Result", "next", LuaResultNext);
        RegisterMethod(luaState, "Result", "free", LuaResultFree);
    }

    private static int LuaResultGetNumber(LuaState luaState)
    {
        // Result.getNumber(id, column)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(luaState, 1));

        if (res is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        var s = GetString(luaState, 2);
        Lua.PushNumber(luaState, res.Get<long>(s));
        return 1;
    }


    private static int LuaResultGetString(LuaState luaState)
    {
        // Result.getString(id, column)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(luaState, 1));

        if (res is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        var s = GetString(luaState, 2);
        Lua.PushString(luaState, res.Get<string>(s));
        return 1;
    }

    private static int LuaResultNext(LuaState luaState)
    {
        // Result.next(id)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(luaState, -1));

        if (res is null)
        {
            PushBoolean(luaState, false);
            return 1;
        }

        PushBoolean(luaState, res.Next());
        return 1;
    }

    private static int LuaResultFree(LuaState luaState)
    {
        // Result.free(id)
        PushBoolean(luaState, GetScriptEnv().RemoveResult(GetNumber<uint>(luaState, -1)));
        return 1;
    }
}
