using LuaNET;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Functions;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaGameManager : ILuaGameManager
{
    #region Members

    #endregion

    #region Injection

    /// <summary>
    /// A reference to the logger instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the lua enviroment instance in use.
    /// </summary>
    private readonly ILuaEnvironment _luaEnviroment;

    /// <summary>
    /// A reference to the config manager instance in use.
    /// </summary>
    private readonly IConfigManager _configManager;

    /// <summary>
    /// A reference to the scripts instance in use.
    /// </summary>
    private readonly IScripts _scripts;

    private readonly ITalkActions _talkActions;

    private readonly IConfigFunctions _configFunctions;
    private readonly ICreatureFunctions _creatureFunctions;
    private readonly IGlobalFunctions _globalFunctions;
    private readonly ILoggerFunctions _loggerFunctions;
    private readonly IPlayerFunctions _playerFunctions;
    private readonly ITalkActionFunctions _talkActionFunctions;

    #endregion

    #region Constructors

    public LuaGameManager(
        ILogger logger,
        ILuaEnvironment luaEnviroment,
        IConfigManager configManager,
        IScripts scripts,
        ITalkActions talkActions,
        IConfigFunctions configFunctions,
        ICreatureFunctions creatureFunctions,
        IGlobalFunctions globalFunctions,
        ILoggerFunctions loggerFunctions,
        IPlayerFunctions playerFunctions,
        ITalkActionFunctions talkActionFunctions)
    {
        _logger = logger;
        _luaEnviroment = luaEnviroment;
        _configManager = configManager;
        _scripts = scripts;
        _talkActions = talkActions;

        _configFunctions = configFunctions;
        _creatureFunctions = creatureFunctions;
        _globalFunctions = globalFunctions;
        _loggerFunctions = loggerFunctions;
        _playerFunctions = playerFunctions;
        _talkActionFunctions = talkActionFunctions;

        Start();
    }

    #endregion

    #region Public Methods 

    public bool PlayerSaySpell(IPlayer player, SpeechType type, string words)
    {
        var wordsSeparator = " ";
        var talkactionWords = words.Contains(wordsSeparator) ? words.Split(" ") : [words];

        if (!talkactionWords.Any())
            return false;

        var talkAction = _talkActions.GetTalkAction(talkactionWords[0]);

        if (talkAction == null)
            return false;

        var parameter = "";

        if (talkactionWords.Count() > 1)
            parameter = talkactionWords[1];

        return talkAction.ExecuteSay(player, talkactionWords[0], parameter, type);
    }

    #endregion

    #region Private Methods

    private void Start()
    {
        var dir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            dir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(_luaEnviroment.InitState(), "luaEnviroment");

        var luaState = _luaEnviroment.GetLuaState();

        if (luaState.IsNull)
        {
            //Game.DieSafely("Invalid lua state, cannot load lua functions.");
            Console.WriteLine("Invalid lua state, cannot load lua functions.");
        }

        Lua.OpenLibs(luaState);

        //TODO: load all functions
        //CoreFunctions.Init(L);
        //EventFunctions.Init(L);
        //ItemFunctions.Init(L);
        //MapFunctions.Init(L);
        //ZoneFunctions.Init(L);
        //GameFunctions.Init(L);
        //GlobalEventFunctions.Init(L);
        //CreatureEventsFunctions.Init(L);

        _configFunctions.Init(luaState);
        _creatureFunctions.Init(luaState);
        _globalFunctions.Init(luaState);
        _loggerFunctions.Init(luaState);
        _playerFunctions.Init(luaState);
        _talkActionFunctions.Init(luaState);

        ModulesLoadHelper(_configManager.Load($"{dir}/config.lua"), $"config.lua");

        ModulesLoadHelper(_luaEnviroment.LoadFile($"{dir}/Data/LuaJit/core.lua", "core.lua"), "core.lua");

        ModulesLoadHelper(_scripts.LoadScripts($"{dir}/Data/LuaJit/scripts", false, false), "/Data/LuaJit/scripts");
    }

    private void ModulesLoadHelper(bool loaded, string moduleName)
    {
        _logger.Information($"Loaded {moduleName}");
        if (!loaded)
            _logger.Error(string.Format("Cannot load: {0}", moduleName));
    }

    #endregion
}
