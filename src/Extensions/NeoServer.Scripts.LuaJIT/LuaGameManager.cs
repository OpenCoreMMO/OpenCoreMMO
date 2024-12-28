using LuaNET;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaGameManager : ILuaGameManager
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
    /// A reference to the <see cref="IActions"/> instance in use.
    /// </summary>
    private readonly IActions _actions;

    /// <summary>
    /// A reference to the <see cref="ITalkActions"/> instance in use.
    /// </summary>
    private readonly ITalkActions _talkActions;

    /// <summary>
    /// A reference to the <see cref="IActionFunctions"/> instance in use.
    /// </summary>
    private readonly IActionFunctions _actionFunctions;

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
    /// A reference to the <see cref="INpcFunctions"/> instance in use.
    /// </summary>
    private readonly INpcFunctions _npcFunctions;

    /// <summary>
    /// A reference to the <see cref="IPlayerFunctions"/> instance in use.
    /// </summary>
    private readonly IPlayerFunctions _playerFunctions;

    /// <summary>
    /// A reference to the <see cref="IPositionFunctions"/> instance in use.
    /// </summary>
    private readonly IPositionFunctions _positionFunctions;

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

    public LuaGameManager(
        ILogger logger,
        ILuaEnvironment luaEnviroment,
        IConfigManager configManager,
        IScripts scripts,
        IActions actions,
        ITalkActions talkActions,
        IActionFunctions actionFunctions,
        IConfigFunctions configFunctions,
        IContainerFunctions containerFunctions,
        ICreatureFunctions creatureFunctions,
        IEnumFunctions enumFunctions,
        IGameFunctions gameFunctions,
        IGlobalFunctions globalFunctions,
        IItemFunctions itemFunctions,
        IItemTypeFunctions itemTypeFunctions,
        ILoggerFunctions loggerFunctions,
        IMonsterFunctions monsterFunctions,
        INpcFunctions npcFunctions,
        IPlayerFunctions playerFunctions,
        IPositionFunctions positionFunctions,
        ITalkActionFunctions talkActionFunctions,
        ITeleportFunctions teleportFunctions,
        ITileFunctions tileFunctions,
        ServerConfiguration serverConfiguration)
    {
        _logger = logger;
        _luaEnviroment = luaEnviroment;
        _configManager = configManager;
        _scripts = scripts;

        _actions = actions;
        _talkActions = talkActions;

        _actionFunctions = actionFunctions;
        _configFunctions = configFunctions;
        _containerFunctions = containerFunctions;
        _creatureFunctions = creatureFunctions;
        _enumFunctions = enumFunctions;
        _gameFunctions = gameFunctions;
        _globalFunctions = globalFunctions;
        _itemFunctions = itemFunctions;
        _itemTypeFunctions = itemTypeFunctions;
        _loggerFunctions = loggerFunctions;
        _playerFunctions = playerFunctions;
        _monsterFunctions = monsterFunctions;
        _npcFunctions = npcFunctions;
        _positionFunctions = positionFunctions;
        _talkActionFunctions = talkActionFunctions;
        _teleportFunctions = teleportFunctions;
        _tileFunctions = tileFunctions;

        _serverConfiguration = serverConfiguration;
    }

    #endregion

    #region Public Methods 

    public void Start()
    {
        var dir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            dir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(_luaEnviroment.InitState(), "luaEnviroment");

        var luaState = _luaEnviroment.GetLuaState();

        if (luaState.IsNull)
            _logger.Error("Invalid lua state, cannot load lua Functions.");

        Lua.OpenLibs(luaState);

        _actionFunctions.Init(luaState);
        _configFunctions.Init(luaState);
        _creatureFunctions.Init(luaState);
        _enumFunctions.Init(luaState);
        _gameFunctions.Init(luaState);
        _globalFunctions.Init(luaState);
        _itemFunctions.Init(luaState);
        _itemTypeFunctions.Init(luaState);
        _loggerFunctions.Init(luaState);
        _positionFunctions.Init(luaState);
        _talkActionFunctions.Init(luaState);
        _tileFunctions.Init(luaState);

        _containerFunctions.Init(luaState);
        _monsterFunctions.Init(luaState);
        _npcFunctions.Init(luaState);
        _playerFunctions.Init(luaState);
        _teleportFunctions.Init(luaState);

        ModulesLoadHelper(_configManager.Load($"{dir}/config.lua"), $"config.lua");

        ModulesLoadHelper(_luaEnviroment.LoadFile($"{dir}{_serverConfiguration.DataLuaJit}/core.lua", "core.lua"), "core.lua");

        ModulesLoadHelper(_scripts.LoadScripts($"{dir}{_serverConfiguration.DataLuaJit}/scripts", false, false), "/Data/LuaJit/scripts");
    }

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

    public bool PlayerUseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item)
    {
        var action = _actions.GetAction(item);

        if (action != null)
            return action.ExecuteUse(
                player,
                item,
                pos,
                null,
                pos,
                false);
        else
            _logger.Warning($"Action with item id {item.ServerId} not found.");

        return false;
    }

    public bool PlayerUseItemWithCreature(IPlayer player, Location fromPos, byte fromStackPos, ICreature creature, IItem item)
    {
        return PlayerUseItemEx(player, fromPos, creature.Location, creature.Tile.GetCreatureStackPositionIndex(player), item, false, creature);
    }

    public bool PlayerUseItemEx(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, bool isHotkey, IThing target)
    {
        var action = _actions.GetAction(item);

        if (action != null)
            return action.ExecuteUse(
                player,
                item,
                player.Location,
                target,
                toPos,
                isHotkey);
        else
            _logger.Warning($"Action with item id {item.ServerId} not found.");

        return false;
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
