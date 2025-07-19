using NeoServer.Domain.Common;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using NeoServer.Server.Configurations;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaScriptManager(
    ILuaStartup luaStartup,
    IGlobalEvents globalEvents,
    ILogger logger,
    IActionScriptService actionsScriptService,
    ICreatureEventsScriptService creatureEventsScriptService,
    IGlobalEventsScriptService globalEventsScriptService,
    IMoveEventsScriptService moveEventsScriptService,
    ITalkActionScriptService talkActionsScriptService,
    IReloadManager reloadManager,
    ServerConfiguration serverConfiguration) : IScriptManager
{
    #region Private Members

    private static FileSystemWatcher _scriptsWatcher;
    private static Timer _debounceTimer;
    private static readonly object _debounceLock = new();
    private static bool _pendingReload;

    #endregion

    #region Properties

    public IActionScriptService Actions { get; } = actionsScriptService;
    public ICreatureEventsScriptService CreatureEvents { get; } = creatureEventsScriptService;
    public IGlobalEventsScriptService GlobalEvents { get; } = globalEventsScriptService;
    public IMoveEventsScriptService MoveEvents { get; } = moveEventsScriptService;
    public ITalkActionScriptService TalkActions { get; } = talkActionsScriptService;

    #endregion

    #region Public Methods

    public void Initialize()
    {
        luaStartup.Start();
        globalEvents.Startup();

        if (serverConfiguration.AutoReloadScripts)
        {
            SetupAutoReloadLuaScripts();
            logger.Information("Auto Reload Lua Scripts is Enabled");
        }
        else
        {
            logger.Information("Auto Reload Lua Scripts is Disabled");
        }
    }

    #endregion

    public void Reload(ReloadType reloadType)
    {
        reloadManager.Reload(reloadType);
        logger.Information("Reloaded {reloadType} Lua Scripts", reloadType);
    }

    #region Private Methods

    private void SetupAutoReloadLuaScripts()
    {
        _scriptsWatcher = new FileSystemWatcher(serverConfiguration.Data, "*.lua");
        _scriptsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;
        _scriptsWatcher.IncludeSubdirectories = true;

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            lock (_debounceLock)
            {
                _pendingReload = true;
                //Reset timer: only reloads if there are no events for 1 second
                _debounceTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                if (_debounceTimer == null)
                {
                    _debounceTimer = new Timer(_ =>
                    {
                        lock (_debounceLock)
                        {
                            if (_pendingReload)
                            {
                                Reload(ReloadType.All);
                                _pendingReload = false;
                            }
                        }
                    }, null, 1000, Timeout.Infinite);
                }
                else
                {
                    _debounceTimer.Change(1000, Timeout.Infinite);
                }
            }
        }

        _scriptsWatcher.Changed += OnFileChanged;
        _scriptsWatcher.Created += OnFileChanged;
        _scriptsWatcher.Deleted += OnFileChanged;
        _scriptsWatcher.Renamed += OnFileChanged;

        _scriptsWatcher.EnableRaisingEvents = true;
    }

    #endregion
}