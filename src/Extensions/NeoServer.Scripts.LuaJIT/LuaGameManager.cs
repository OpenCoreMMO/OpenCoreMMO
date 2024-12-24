using LuaNET;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;
using NeoServer.Server.Configurations;
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

    private readonly IActionLuaMapping _actionLuaMapping;
    private readonly IConfigLuaMapping _configLuaMapping;
    private readonly ICreatureLuaMapping _creatureLuaMapping;
    private readonly IEnumLuaMapping _enumLuaMapping;
    private readonly IGameLuaMapping _gameLuaMapping;
    private readonly IGlobalLuaMapping _globalLuaMapping;
    private readonly IItemLuaMapping _itemLuaMapping;
    private readonly IItemTypeLuaMapping _itemTypeLuaMapping;
    private readonly ILoggerLuaMapping _loggerLuaMapping;
    private readonly IPlayerLuaMapping _playerLuaMapping;
    private readonly IPositionLuaMapping _positionLuaMapping;
    private readonly ITalkActionLuaMapping _talkActionLuaMapping;
    private readonly ITileLuaMapping _tileLuaMapping;

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
        IActionLuaMapping actionLuaMapping,
        IConfigLuaMapping configLuaMapping,
        ICreatureLuaMapping creatureLuaMapping,
        IEnumLuaMapping enumLuaMapping,
        IGameLuaMapping gameLuaMapping,
        IGlobalLuaMapping globalLuaMapping,
        IItemLuaMapping itemLuaMapping,
        IItemTypeLuaMapping itemTypeLuaMapping,
        ILoggerLuaMapping loggerLuaMapping,
        IPlayerLuaMapping playerLuaMapping,
        IPositionLuaMapping positionLuaMapping,
        ITalkActionLuaMapping talkActionLuaMapping,
        ITileLuaMapping tileLuaMapping,
        ServerConfiguration serverConfiguration)
    {
        _logger = logger;
        _luaEnviroment = luaEnviroment;
        _configManager = configManager;
        _scripts = scripts;

        _actions = actions;
        _talkActions = talkActions;

        _actionLuaMapping = actionLuaMapping;
        _configLuaMapping = configLuaMapping;
        _creatureLuaMapping = creatureLuaMapping;
        _enumLuaMapping = enumLuaMapping;
        _gameLuaMapping = gameLuaMapping;
        _globalLuaMapping = globalLuaMapping;
        _itemLuaMapping = itemLuaMapping;
        _itemTypeLuaMapping = itemTypeLuaMapping;
        _loggerLuaMapping = loggerLuaMapping;
        _playerLuaMapping = playerLuaMapping;
        _positionLuaMapping = positionLuaMapping;
        _talkActionLuaMapping = talkActionLuaMapping;
        _tileLuaMapping = tileLuaMapping;

        _serverConfiguration = serverConfiguration;
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

    public void Start()
    {
        var dir = AppContext.BaseDirectory;

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            dir = ArgManager.GetInstance().ExePath;

        ModulesLoadHelper(_luaEnviroment.InitState(), "luaEnviroment");

        var luaState = _luaEnviroment.GetLuaState();

        if (luaState.IsNull)
            _logger.Error("Invalid lua state, cannot load lua LuaMapping.");

        Lua.OpenLibs(luaState);

        _actionLuaMapping.Init(luaState);
        _configLuaMapping.Init(luaState);
        _creatureLuaMapping.Init(luaState);
        _enumLuaMapping.Init(luaState);
        _gameLuaMapping.Init(luaState);
        _globalLuaMapping.Init(luaState);
        _itemLuaMapping.Init(luaState);
        _itemTypeLuaMapping.Init(luaState);
        _loggerLuaMapping.Init(luaState);
        _playerLuaMapping.Init(luaState);
        _positionLuaMapping.Init(luaState);
        _talkActionLuaMapping.Init(luaState);
        _tileLuaMapping.Init(luaState);

        ModulesLoadHelper(_configManager.Load($"{dir}/config.lua"), $"config.lua");

        ModulesLoadHelper(_luaEnviroment.LoadFile($"{dir}{_serverConfiguration.DataLuaJit}/core.lua", "core.lua"), "core.lua");

        ModulesLoadHelper(_scripts.LoadScripts($"{dir}{_serverConfiguration.DataLuaJit}/scripts", false, false), "/Data/LuaJit/scripts");
    }

    private void ModulesLoadHelper(bool loaded, string moduleName)
    {
        _logger.Information($"Loaded {moduleName}");
        if (!loaded)
            _logger.Error(string.Format("Cannot load: {0}", moduleName));
    }

    #endregion
}
