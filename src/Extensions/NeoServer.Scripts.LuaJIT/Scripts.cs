using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Spells;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class Scripts : IScripts
{
    #region injection

    /// <summary>
    ///     A reference to the logger in use.
    /// </summary>
    protected readonly ILogger _logger;

    /// <summary>
    ///     A reference to the config manager in use.
    /// </summary>
    private readonly IConfigManager _configManager;

    private readonly IActions _actions;
    private readonly ICreatureEvents _creatureEvents;
    private readonly IGlobalEvents _globalEvents;
    private readonly IMoveEvents _moveEvents;
    private readonly INpcs _npcs;

    /// <summary>
    ///     A reference to the talk actions instance in use.
    /// </summary>
    private readonly ITalkActions _talkActions;

    private readonly SpellListManager _spellListManager;
    private readonly INpcTypeStore _npcStore;
    private readonly IEventsCallbacks _eventsCallbacks;

    #endregion

    #region members

    private readonly int _scriptId = 0;
    private readonly LuaScriptInterface _scriptInterface;

    #endregion

    #region constructors
    
    public Scripts(ILogger logger)
    {
        _logger = logger;

        _scriptInterface = new LuaScriptInterface("Scripts Interface");
        _scriptInterface.InitState();
    }

    public Scripts(
        ILogger logger,
        IConfigManager configManager,
        ITalkActions talkActions,
        IActions actions,
        ICreatureEvents creatureEvents,
        IGlobalEvents globalEvents,
        IMoveEvents moveEvents,
        INpcs npcs,
        SpellListManager spellListManager,
        INpcTypeStore npcStore,
        IEventsCallbacks eventsCallbacks)
    {
        //_instance = this;

        _logger = logger.ForContext<Scripts>();
        _configManager = configManager;
        _actions = actions;
        _creatureEvents = creatureEvents;
        _globalEvents = globalEvents;
        _moveEvents = moveEvents;
        _npcs = npcs;
        _talkActions = talkActions;

        _scriptInterface = new LuaScriptInterface("Scripts Interface");
        _spellListManager = spellListManager;
        _npcStore = npcStore;
        _eventsCallbacks = eventsCallbacks;
        //_scriptInterface.InitState();
    }

    #endregion

    #region public methods implementation

    public void ClearAllScripts()
    {
        _actions.Clear();
        _creatureEvents.Clear();
        _globalEvents.Clear();
        _moveEvents.Clear();
        _npcs.Clear();
        _talkActions.Clear();
        _spellListManager.Clear();
        _npcStore.Clear();
        _eventsCallbacks.Clear();
    }

    public bool LoadEventSchedulerScripts(string fileName)
    {
        var coreFolder = _configManager.GetString(StringConfigType.CORE_DIRECTORY);

        var dir = Directory.GetCurrentDirectory();

        if (!string.IsNullOrEmpty(ArgManager.GetInstance().ExePath))
            dir = ArgManager.GetInstance().ExePath;

        dir = Path.Combine(dir, coreFolder, "events", "scripts", "scheduler");

        if (!Directory.Exists(dir) || !Directory.GetDirectories(dir).Any())
        {
            _logger.Warning(
                "{LoadEventSchedulerScriptsName} - Can not load folder \'scheduler\' on {CoreFolder}/events/scripts\'",
                nameof(LoadEventSchedulerScripts), coreFolder);
            return false;
        }

        foreach (var filePath in Directory.GetFiles(dir, "*.lua", SearchOption.AllDirectories))
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Name == fileName)
            {
                if (!_scriptInterface.LoadFile(fileInfo.FullName, fileInfo.Name))
                {
                    _logger.Error("File: {FullName} - Error: {Error}", fileInfo.FullName,
                        _scriptInterface.GetLastLuaError());
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    public bool LoadScripts(string loadPath, bool isLib, bool reload)
    {
        if (_scriptInterface.GetLuaState().pointer == 0)
            _scriptInterface.InitState();

        if (!Directory.Exists(loadPath))
        {
            _logger.Error("Can not load folder {LoadPath}", loadPath);
            return false;
        }

        string lastDirectory = null;

        var searchOption = SearchOption.AllDirectories;

        if (!Directory.GetDirectories(loadPath).Any())
            searchOption = SearchOption.TopDirectoryOnly;

        foreach (var filePath in Directory.GetFiles(loadPath, "*.lua", searchOption))
        {
            var fileInfo = new FileInfo(filePath);
            var fileFolder = fileInfo.DirectoryName?.Split(Path.DirectorySeparatorChar).LastOrDefault();
            var scriptFolder = fileInfo.DirectoryName;

            if (!File.Exists(filePath) || fileInfo.Extension != ".lua") continue;

            if (fileInfo.Name.StartsWith("#"))
            {
                if (_configManager.GetBoolean(BooleanConfigType.SCRIPTS_CONSOLE_LOGS))
                    _logger.Information("[script]: {} [disabled]", fileInfo.Name);

                continue;
            }

            if (isLib || (fileFolder != "lib" && fileFolder != "events"))
            {
                if (_configManager.GetBoolean(BooleanConfigType.SCRIPTS_CONSOLE_LOGS))
                {
                    if (string.IsNullOrEmpty(lastDirectory) || lastDirectory != scriptFolder)
                        _logger.Information("Loading folder: [{LastOrDefault}]",
                            fileInfo.DirectoryName?.Split(Path.DirectorySeparatorChar).LastOrDefault());

                    lastDirectory = fileInfo.DirectoryName;
                }

                if (!_scriptInterface.LoadFile(fileInfo.FullName, fileInfo.Name))
                {
                    _logger.Error("File: {FullName} - Error: {Error}", fileInfo.FullName,
                        _scriptInterface.GetLastLuaError());
                    continue;
                }
            }

            if (_configManager.GetBoolean(BooleanConfigType.SCRIPTS_CONSOLE_LOGS))
            {
                if (!reload)
                    _logger.Information("[script loaded]: {Name}", fileInfo.Name);
                else
                    _logger.Information("[script reloaded]: {Name}", fileInfo.Name);
            }
        }

        return true;
    }

    public LuaScriptInterface GetScriptInterface()
    {
        return _scriptInterface;
    }

    public int GetScriptId()
    {
        return _scriptId;
    }

    #endregion
}