using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Extensions;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class GameFunctions : LuaScriptInterface, IGameFunctions
{
    private static IScripts _scripts;
    private static ILogger _logger;

    public GameFunctions(
        ILuaEnvironment luaEnvironment, 
        ILogger logger, 
        IScripts scripts) : base(nameof(GameFunctions))
    {
        _scripts = scripts;
        _logger = logger;
    }

    public void Init(LuaState L)
    {
        RegisterTable(L, "Game");
        RegisterMethod(L, "Game", "getReturnMessage", LuaGameGetReturnMessage);
        RegisterMethod(L, "Game", "reload", LuaGameReload);
    }

    private static int LuaGameGetReturnMessage(LuaState L)
    {
        // Game.getReturnMessage(value)
        var returnValue = GetNumber<ReturnValueType>(L, 1);
        PushString(L, returnValue.GetReturnMessage());
        return 1;
    }

    private static int LuaGameReload(LuaState L)
    {
        // Game.reload(reloadType)
        var reloadType = GetNumber<ReloadType>(L, 1);
        if (reloadType == ReloadType.RELOAD_TYPE_NONE)
        {
            //reportErrorFunc("Reload type is none");
            PushBoolean(L, false);
            return 0;
        }

        if (reloadType >= ReloadType.RELOAD_TYPE_LAST)
        {
            //reportErrorFunc("Reload type not exist");
            PushBoolean(L, false);
            return 0;
        }
        try
        {
            _logger.Information("Reloading Scripts");
            var dir = AppContext.BaseDirectory;
            _scripts.ClearAllScripts();
            _scripts.LoadScripts($"{dir}/Data/LuaJit/scripts", false, true);
            _logger.Information("Reloaded Scripts");
        }
        catch (Exception e)
        {
            PushBoolean(L, false);
            return 0;
        }

        PushBoolean(L, true);
        return 1;
    }
}
