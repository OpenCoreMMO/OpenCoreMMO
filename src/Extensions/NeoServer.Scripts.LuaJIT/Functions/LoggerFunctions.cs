using LuaNET;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class LoggerFunctions : LuaScriptInterface, ILoggerFunctions
{
    private static ILogger _logger;

    public LoggerFunctions(ILogger logger) : base(nameof(LoggerFunctions))
    {
        _logger = logger;
    }

    public void Init(LuaState L)
    {
        RegisterTable(L, "logger");
        RegisterMethod(L, "logger", "info", LuaLoggerInfo);
        RegisterMethod(L, "logger", "warn", LuaLoggerWarn);
        RegisterMethod(L, "logger", "error", LuaLoggerError);
        RegisterMethod(L, "logger", "debug", LuaLoggerDebug);
    }

    private static int LuaLoggerInfo(LuaState L)
    {
        // logger.info(text)
        if (IsString(L, 1))
        {
            _logger.Information(GetFormatedLoggerMessage(L));
        }
        else
        {
            //reportErrorFunc("First parameter needs to be a string");
        }
        return 1;
    }

    private static int LuaLoggerWarn(LuaState L)
    {
        // logger.info(text)
        if (IsString(L, 1))
        {
            _logger.Warning(GetFormatedLoggerMessage(L));
        }
        else
        {
            //reportErrorFunc("First parameter needs to be a string");
        }
        return 1;
    }

    private static int LuaLoggerError(LuaState L)
    {
        // logger.info(text)
        if (IsString(L, 1))
        {
            _logger.Error(GetFormatedLoggerMessage(L));
        }
        else
        {
            //reportErrorFunc("First parameter needs to be a string");
        }
        return 1;
    }

    private static int LuaLoggerDebug(LuaState L)
    {
        // logger.info(text)
        if (IsString(L, 1))
        {
            _logger.Debug(GetFormatedLoggerMessage(L));
        }
        else
        {
            //reportErrorFunc("First parameter needs to be a string");
        }
        return 1;
    }
}
