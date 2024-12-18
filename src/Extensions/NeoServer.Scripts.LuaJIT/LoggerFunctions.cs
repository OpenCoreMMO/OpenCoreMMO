using LuaNET;

namespace NeoServer.Scripts.LuaJIT;

public class LoggerFunctions : LuaScriptInterface
{
    public LoggerFunctions() : base(nameof(LoggerFunctions))
    {
    }

    public static void Init(LuaState L)
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
            Logger.GetInstance().Info(GetFormatedLoggerMessage(L));
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
            Logger.GetInstance().Warn(GetFormatedLoggerMessage(L));
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
            Logger.GetInstance().Error(GetFormatedLoggerMessage(L));
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
            Logger.GetInstance().Debug(GetFormatedLoggerMessage(L));
        }
        else
        {
            //reportErrorFunc("First parameter needs to be a string");
        }
        return 1;
    }
}
