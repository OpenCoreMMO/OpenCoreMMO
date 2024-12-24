using LuaNET;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
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

    private readonly IActions _actions;
    private readonly ITalkActions _talkActions;

    private readonly IActionFunctions _actionFunctions;
    private readonly IConfigFunctions _configFunctions;
    private readonly ICreatureFunctions _creatureFunctions;
    private readonly IEnumFunctions _enumFunctions;
    private readonly IGameFunctions _gameFunctions;
    private readonly IGlobalFunctions _globalFunctions;
    private readonly IItemFunctions _itemFunctions;
    private readonly IItemTypeFunctions _itemTypeFunctions;
    private readonly ILoggerFunctions _loggerFunctions;
    private readonly IPlayerFunctions _playerFunctions;
    private readonly IPositionFunctions _positionFunctions;
    private readonly ITalkActionFunctions _talkActionFunctions;
    private readonly ITileFunctions _tileFunctions;

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
        ICreatureFunctions creatureFunctions,
        IEnumFunctions enumFunctions,
        IGameFunctions gameFunctions,
        IGlobalFunctions globalFunctions,
        IItemFunctions itemFunctions,
        IItemTypeFunctions itemTypeFunctions,
        ILoggerFunctions loggerFunctions,
        IPlayerFunctions playerFunctions,
        IPositionFunctions positionFunctions,
        ITalkActionFunctions talkActionFunctions,
        ITileFunctions tileFunctions)
    {
        _logger = logger;
        _luaEnviroment = luaEnviroment;
        _configManager = configManager;
        _scripts = scripts;

        _actions = actions;
        _talkActions = talkActions;

        _actionFunctions = actionFunctions;
        _configFunctions = configFunctions;
        _creatureFunctions = creatureFunctions;
        _enumFunctions = enumFunctions;
        _gameFunctions = gameFunctions;
        _globalFunctions = globalFunctions;
        _itemFunctions = itemFunctions;
        _itemTypeFunctions = itemTypeFunctions;
        _loggerFunctions = loggerFunctions;
        _playerFunctions = playerFunctions;
        _positionFunctions = positionFunctions;
        _talkActionFunctions = talkActionFunctions;
        _tileFunctions = tileFunctions;

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

    private void Start()
    {
        var dir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            dir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(_luaEnviroment.InitState(), "luaEnviroment");

        var luaState = _luaEnviroment.GetLuaState();

        if (luaState.IsNull)
            _logger.Error("Invalid lua state, cannot load lua functions.");

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
        _playerFunctions.Init(luaState);
        _positionFunctions.Init(luaState);
        _talkActionFunctions.Init(luaState);
        _tileFunctions.Init(luaState);

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
