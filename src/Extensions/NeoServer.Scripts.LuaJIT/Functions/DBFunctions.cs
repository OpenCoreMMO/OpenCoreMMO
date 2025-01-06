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

    public void Init(LuaState L)
    {
        RegisterTable(L, "db");
        RegisterMethod(L, "db", "query", LuaDatabaseQuery);
        RegisterMethod(L, "db", "asyncQuery", LuaDatabaseAsyncQuery);
        RegisterMethod(L, "db", "storeQuery", LuaDatabaseStoreQuery);
        RegisterMethod(L, "db", "asyncStoreQuery", LuaDatabaseAsyncStoreQuery);
        RegisterMethod(L, "db", "escapeString", LuaDatabaseEscapeString);
        RegisterMethod(L, "db", "tableExists", LuaDatabaseTableExists);
    }

    private static int LuaDatabaseQuery(LuaState L)
    {
        // db.query(query)
        var query = GetString(L, -1);
        var result = _dbContext.Database.ExecuteSqlRaw(query);
        PushBoolean(L, result != 0);
        return 1;
    }

    private static int LuaDatabaseAsyncQuery(LuaState L)
    {
        // db.queryAsync(query)
        var query = GetString(L, -1);
        var result = _dbContext.Database.ExecuteSqlRawAsync(query).Result;
        PushBoolean(L, result != 0);
        return 1;
    }

    private static int LuaDatabaseStoreQuery(LuaState L)
    {
        // db.storeQuery(query)
        var query = GetString(L, -1);

        var dbResult = _dbContext.ExecuteQuery(query);

        if (dbResult != null)
            Lua.PushNumber(L, GetScriptEnv().AddResult(dbResult));
        else
            PushBoolean(L, false);
        return 1;
    }

    private static int LuaDatabaseAsyncStoreQuery(LuaState L)
    {
        // db.asyncStoreQueryAsync(query)
        var query = GetString(L, -1);

        var dbResult = _dbContext.ExecuteQueryAsync(query);
        
        if (dbResult != null)
            Lua.PushNumber(L, GetScriptEnv().AddResult(dbResult.Result));
        else
            PushBoolean(L, false);

        return 1;
    }

    private static int LuaDatabaseEscapeString(LuaState L)
    {
        // db.escapeString(value)
        var value = GetString(L, -1);
        var result = EscapeString(value);
        PushString(L, result);
        return 1;
    }

    private static int LuaDatabaseTableExists(LuaState L)
    {
        // db.tableExists(name)
        var name = GetString(L, -1);
        PushBoolean(L, _dbContext.TableExists(name));
        return 1;
    }
}
