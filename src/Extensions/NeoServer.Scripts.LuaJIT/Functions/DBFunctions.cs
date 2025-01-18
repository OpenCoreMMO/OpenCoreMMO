using LuaNET;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Tasks;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class DBFunctions : LuaScriptInterface, IDBFunctions
{
    private static ILogger _logger;
    private static IScheduler _scheduler;
    private static ILuaEnvironment _luaEnvironment;
    private static NeoContext _dbContext;

    public DBFunctions(
        ILogger logger,
        IScheduler scheduler,
        ILuaEnvironment luaEnvironment,
        NeoContext dbContext) : base(nameof(DBFunctions))

    {
        _logger = logger;
        _scheduler = scheduler;
        _luaEnvironment = luaEnvironment;
        _dbContext = dbContext;
    }

    public void Init(LuaState luaState)
    {
        RegisterTable(luaState, "db");
        RegisterMethod(luaState, "db", "query", LuaDatabaseQuery);
        RegisterMethod(luaState, "db", "asyncQuery", LuaDatabaseAsyncQuery);
        RegisterMethod(luaState, "db", "storeQuery", LuaDatabaseStoreQuery);
        RegisterMethod(luaState, "db", "asyncStoreQuery", LuaDatabaseAsyncStoreQuery);
        RegisterMethod(luaState, "db", "escapeString", LuaDatabaseEscapeString);
        RegisterMethod(luaState, "db", "tableExists", LuaDatabaseTableExists);
    }

    private static int LuaDatabaseQuery(LuaState luaState)
    {
        // db.query(query)
        var query = GetString(luaState, -1);
        var result = _dbContext.Database.ExecuteSqlRaw(query);
        PushBoolean(luaState, result != 0);
        return 1;
    }

    private static int LuaDatabaseAsyncQuery(LuaState luaState)
    {
        // db.queryAsync(query)
        var query = GetString(luaState, -1);
        var result = _dbContext.Database.ExecuteSqlRawAsync(query).Result;
        PushBoolean(luaState, result != 0);
        return 1;
    }

    private static int LuaDatabaseStoreQuery(LuaState luaState)
    {
        // db.storeQuery(query)
        var query = GetString(luaState, -1);

        var dbResult = _dbContext.ExecuteQuery(query);

        if (dbResult != null)
            Lua.PushNumber(luaState, GetScriptEnv().AddResult(dbResult));
        else
            PushBoolean(luaState, false);
        return 1;
    }

    private static int LuaDatabaseAsyncStoreQuery(LuaState luaState)
    {
        // db.asyncStoreQueryAsync(query)
        var query = GetString(luaState, -1);

        var dbResult = _dbContext.ExecuteQueryAsync(query);
        
        if (dbResult != null)
            Lua.PushNumber(luaState, GetScriptEnv().AddResult(dbResult.Result));
        else
            PushBoolean(luaState, false);

        return 1;
    }

    private static int LuaDatabaseEscapeString(LuaState luaState)
    {
        // db.escapeString(value)
        var value = GetString(luaState, -1);
        var result = EscapeString(value);
        PushString(luaState, result);
        return 1;
    }

    private static int LuaDatabaseTableExists(LuaState luaState)
    {
        // db.tableExists(name)
        var name = GetString(luaState, -1);
        PushBoolean(luaState, _dbContext.TableExists(name));
        return 1;
    }
}
