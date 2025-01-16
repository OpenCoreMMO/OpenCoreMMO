using LuaNET;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class LoggerFunctions : LuaScriptInterface, ILoggerFunctions
{
    private static ILogger _logger;

    public LoggerFunctions(
        ILuaEnvironment luaEnvironment, ILogger logger) : base(nameof(LoggerFunctions))
    {
        _logger = logger;
    }

    public void Init(LuaState luaState)
    {
        RegisterTable(luaState, "logger");
        RegisterMethod(luaState, "logger", "info", LuaLoggerInfo);
        RegisterMethod(luaState, "logger", "warn", LuaLoggerWarn);
        RegisterMethod(luaState, "logger", "error", LuaLoggerError);
        RegisterMethod(luaState, "logger", "debug", LuaLoggerDebug);
    }

    private static int LuaLoggerInfo(LuaState luaState)
    {
        // logger.info(text)
        if (IsString(luaState, 1))
            _logger.Information(GetString(luaState, 1), GetFormatedLoggerMessage(luaState));
        else
            ReportError(nameof(LuaLoggerWarn), "First parameter needs to be a string");
        return 1;
    }

    private static int LuaLoggerWarn(LuaState luaState)
    {
        // logger.info(text)
        if (IsString(luaState, 1))
            _logger.Warning(GetString(luaState, 1), GetFormatedLoggerMessage(luaState));
        else
            ReportError(nameof(LuaLoggerWarn), "First parameter needs to be a string");
        return 1;
    }

    private static int LuaLoggerError(LuaState luaState)
    {
        // logger.info(text)
        if (IsString(luaState, 1))
            _logger.Error(GetString(luaState, 1), GetFormatedLoggerMessage(luaState));
        else
            ReportError(nameof(LuaLoggerError), "First parameter needs to be a string");
        return 1;
    }

    private static int LuaLoggerDebug(LuaState luaState)
    {
        // logger.info(text)
        if (IsString(luaState, 1))
            _logger.Debug(GetString(luaState, 1), "1", 1,3 ,4 ,5);
        else
            ReportError(nameof(LuaLoggerDebug), "First parameter needs to be a string");
        return 1;
    }
}