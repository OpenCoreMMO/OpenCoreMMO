using LuaNET;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaStartup : ILuaStartup
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="ILuaEnvironment"/> instance in use.
    /// </summary>
    private readonly ILuaEnvironment _luaEnviroment;

    /// <summary>
    /// A reference to the <see cref="IConfigManager"/> instance in use.
    /// </summary>
    private readonly IConfigManager _configManager;

    /// <summary>
    /// A reference to the <see cref="IScripts"/> instance in use.
    /// </summary>
    private readonly IScripts _scripts;

    /// <summary>
    /// A reference to the <see cref="IActionFunctions"/> instance in use.
    /// </summary>
    private readonly IActionFunctions _actionFunctions;

    /// <summary>
    /// A reference to the <see cref="IConditionFunctions"/> instance in use.
    /// </summary>
    private readonly IConditionFunctions _conditionFunctions;

    /// <summary>
    /// A reference to the <see cref="IConfigFunctions"/> instance in use.
    /// </summary>
    private readonly IConfigFunctions _configFunctions;

    /// <summary>
    /// A reference to the <see cref="IContainerFunctions"/> instance in use.
    /// </summary>
    private readonly IContainerFunctions _containerFunctions;

    /// <summary>
    /// A reference to the <see cref="ICreatureFunctions"/> instance in use.
    /// </summary>
    private readonly ICreatureFunctions _creatureFunctions;

    /// <summary>
    /// A reference to the <see cref="ICreatureEventFunctions"/> instance in use.
    /// </summary>
    private readonly ICreatureEventFunctions _creatureEventFunctions;

    /// <summary>
    /// A reference to the <see cref="IDBFunctions"/> instance in use.
    /// </summary>
    private readonly IDBFunctions _dbFunctions;

    /// <summary>
    /// A reference to the <see cref="IEnumFunctions"/> instance in use.
    /// </summary>
    private readonly IEnumFunctions _enumFunctions;

    /// <summary>
    /// A reference to the <see cref="IGameFunctions"/> instance in use.
    /// </summary>
    private readonly IGameFunctions _gameFunctions;

    /// <summary>
    /// A reference to the <see cref="IGlobalFunctions"/> instance in use.
    /// </summary>
    private readonly IGlobalFunctions _globalFunctions;

    /// <summary>
    /// A reference to the <see cref="IGlobalEventFunctions"/> instance in use.
    /// </summary>
    private readonly IGlobalEventFunctions _globalEventFunctions;

    /// <summary>
    /// A reference to the <see cref="IGroupFunctions"/> instance in use.
    /// </summary>
    private readonly IGroupFunctions _groupFunctions;

    /// <summary>
    /// A reference to the <see cref="IItemFunctions"/> instance in use.
    /// </summary>
    private readonly IItemFunctions _itemFunctions;

    /// <summary>
    /// A reference to the <see cref="IItemTypeFunctions"/> instance in use.
    /// </summary>
    private readonly IItemTypeFunctions _itemTypeFunctions;

    /// <summary>
    /// A reference to the <see cref="ILoggerFunctions"/> instance in use.
    /// </summary>
    private readonly ILoggerFunctions _loggerFunctions;

    /// <summary>
    /// A reference to the <see cref="IMonsterFunctions"/> instance in use.
    /// </summary>
    private readonly IMonsterFunctions _monsterFunctions;

    /// <summary>
    /// A reference to the <see cref="IMoveEventFunctions"/> instance in use.
    /// </summary>
    private readonly IMoveEventFunctions _moveEventFunctions;

    /// <summary>
    /// A reference to the <see cref="INpcFunctions"/> instance in use.
    /// </summary>
    private readonly INpcFunctions _npcFunctions;

    /// <summary>
    /// A reference to the <see cref="INpcTypeFunctions"/> instance in use.
    /// </summary>
    private readonly INpcTypeFunctions _npcTypeFunctions;

    /// <summary>
    /// A reference to the <see cref="IPlayerFunctions"/> instance in use.
    /// </summary>
    private readonly IPlayerFunctions _playerFunctions;

    /// <summary>
    /// A reference to the <see cref="IPositionFunctions"/> instance in use.
    /// </summary>
    private readonly IPositionFunctions _positionFunctions;

    /// <summary>
    /// A reference to the <see cref="IResultFunctions"/> instance in use.
    /// </summary>
    private readonly IResultFunctions _resultFunctions;

    /// <summary>
    /// A reference to the <see cref="ITalkActionFunctions"/> instance in use.
    /// </summary>
    private readonly ITalkActionFunctions _talkActionFunctions;

    /// <summary>
    /// A reference to the <see cref="ITeleportFunctions"/> instance in use.
    /// </summary>
    private readonly ITeleportFunctions _teleportFunctions;

    /// <summary>
    /// A reference to the <see cref="ITileFunctions"/> instance in use.
    /// </summary>
    private readonly ITileFunctions _tileFunctions;

    /// <summary>
    /// A reference to the <see cref="ServerConfiguration"/> instance in use.
    /// </summary>
    private readonly ServerConfiguration _serverConfiguration;

    #endregion

    #region Constructors

    public LuaStartup(
        ILogger logger,
        ILuaEnvironment luaEnviroment,
        IConfigManager configManager,
        IScripts scripts,
        IActionFunctions actionFunctions,
        IConfigFunctions configFunctions,
        IContainerFunctions containerFunctions,
        ICreatureFunctions creatureFunctions,
        ICreatureEventFunctions creatureEventFunctions,
        IDBFunctions dbFunctions,
        IEnumFunctions enumFunctions,
        IGameFunctions gameFunctions,
        IGlobalFunctions globalFunctions,
        IGlobalEventFunctions globalEventFunctions,
        IGroupFunctions groupFunctions,
        IItemFunctions itemFunctions,
        IItemTypeFunctions itemTypeFunctions,
        ILoggerFunctions loggerFunctions,
        IMonsterFunctions monsterFunctions,
        IMoveEventFunctions moveEventFunctions,
        INpcFunctions npcFunctions,
        INpcTypeFunctions npcTypeFunctions,
        IPlayerFunctions playerFunctions,
        IPositionFunctions positionFunctions,
        IResultFunctions resultFunctions,
        ITalkActionFunctions talkActionFunctions,
        ITeleportFunctions teleportFunctions,
        ITileFunctions tileFunctions,
        ServerConfiguration serverConfiguration,
        IConditionFunctions conditionFunctions)
    {
        _logger = logger;
        _luaEnviroment = luaEnviroment;
        _configManager = configManager;
        _scripts = scripts;

        _actionFunctions = actionFunctions;
        _configFunctions = configFunctions;
        _containerFunctions = containerFunctions;
        _creatureFunctions = creatureFunctions;
        _creatureEventFunctions = creatureEventFunctions;
        _dbFunctions = dbFunctions;
        _enumFunctions = enumFunctions;
        _gameFunctions = gameFunctions;
        _globalFunctions = globalFunctions;
        _globalEventFunctions = globalEventFunctions;
        _groupFunctions = groupFunctions;
        _itemFunctions = itemFunctions;
        _itemTypeFunctions = itemTypeFunctions;
        _loggerFunctions = loggerFunctions;
        _playerFunctions = playerFunctions;
        _monsterFunctions = monsterFunctions;
        _moveEventFunctions = moveEventFunctions;
        _npcFunctions = npcFunctions;
        _npcTypeFunctions = npcTypeFunctions;
        _positionFunctions = positionFunctions;
        _resultFunctions = resultFunctions;
        _talkActionFunctions = talkActionFunctions;
        _teleportFunctions = teleportFunctions;
        _tileFunctions = tileFunctions;

        _serverConfiguration = serverConfiguration;
        _conditionFunctions = conditionFunctions;
    }

    #endregion

    #region Public Methods 

    public void Start()
    {
        var currentDir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            currentDir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(_luaEnviroment.InitState(), "luaEnviroment");

        var luaState = _luaEnviroment.GetLuaState();

        if (luaState.IsNull)
            _logger.Error("Invalid lua state, cannot load lua Functions.");

        Lua.OpenLibs(luaState);

        _actionFunctions.Init(luaState);
        _conditionFunctions.Init(luaState);
        _configFunctions.Init(luaState);
        _creatureFunctions.Init(luaState);
        _creatureEventFunctions.Init(luaState);
        _dbFunctions.Init(luaState);
        _enumFunctions.Init(luaState);
        _gameFunctions.Init(luaState);
        _globalFunctions.Init(luaState);
        _globalEventFunctions.Init(luaState);
        _itemFunctions.Init(luaState);
        _itemTypeFunctions.Init(luaState);
        _loggerFunctions.Init(luaState);
        _positionFunctions.Init(luaState);
        _resultFunctions.Init(luaState);
        _talkActionFunctions.Init(luaState);
        _tileFunctions.Init(luaState);

        _containerFunctions.Init(luaState);
        _monsterFunctions.Init(luaState);
        _moveEventFunctions.Init(luaState);
        _npcFunctions.Init(luaState);
        _npcTypeFunctions.Init(luaState);
        _playerFunctions.Init(luaState);
        _teleportFunctions.Init(luaState);
        _groupFunctions.Init(luaState);

        ModulesLoadHelper(_configManager.Load($"{currentDir}/config.lua"), $"config.lua");

        ModulesLoadHelper(_luaEnviroment.LoadFile($"{_serverConfiguration.Data}/core.lua", "core.lua"), "/Data/core.lua");

        ModulesLoadHelper(_scripts.LoadScripts($"{_serverConfiguration.Data}/scripts/libs", true, false), "/Data/scripts/libs");
        ModulesLoadHelper(_scripts.LoadScripts($"{_serverConfiguration.Data}/scripts", false, false), "/Data/scripts");
        ModulesLoadHelper(_luaEnviroment.LoadFile($"{_serverConfiguration.Data}/npclib/load.lua", "load.lua"), "/Data/npclib");

        ModulesLoadHelper(_scripts.LoadScripts($"{_serverConfiguration.Data}/npcs", false, false), "/Data/npcs");
    }

    #endregion

    #region Private Methods

    private void ModulesLoadHelper(bool loaded, string moduleName)
    {
        _logger.Information($"Loaded {moduleName}");
        if (!loaded)
            _logger.Error(string.Format("Cannot load: {0}", moduleName));
    }

    #endregion
}
