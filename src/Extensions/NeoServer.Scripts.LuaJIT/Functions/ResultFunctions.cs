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

    public void Init(LuaState L)
    {
        RegisterTable(L, "Result");
        RegisterMethod(L, "Result", "getNumber", LuaResultGetNumber);
        RegisterMethod(L, "Result", "getString", LuaResultGetString);
        RegisterMethod(L, "Result", "next", LuaResultNext);
        RegisterMethod(L, "Result", "free", LuaResultFree);
    }

    private static int LuaResultGetNumber(LuaState L)
    {
        // Result.getNumber(id, column)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(L, 1));

        if (res is null)
        {
            PushBoolean(L, false);
            return 1;
        }

        var s = GetString(L, 2);
        Lua.PushNumber(L, res.Get<long>(s));
        return 1;
    }


    private static int LuaResultGetString(LuaState L)
    {
        // Result.getString(id, column)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(L, 1));

        if (res is null)
        {
            PushBoolean(L, false);
            return 1;
        }

        var s = GetString(L, 2);
        Lua.PushString(L, res.Get<string>(s));
        return 1;
    }

    private static int LuaResultNext(LuaState L)
    {
        // Result.next(id)
        var res = GetScriptEnv().GetResultByID(GetNumber<uint>(L, -1));

        if (res is null)
        {
            PushBoolean(L, false);
            return 1;
        }

        PushBoolean(L, res.Next());
        return 1;
    }

    private static int LuaResultFree(LuaState L)
    {
        // Result.free(id)
        PushBoolean(L, GetScriptEnv().RemoveResult(GetNumber<uint>(L, -1)));
        return 1;
    }
}
