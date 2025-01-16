using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaScriptGameManager : IScriptGameManager
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILuaStartup"/> instance in use.
    /// </summary>
    private readonly ILuaStartup _luaStartup;

    /// <summary>
    /// A reference to the <see cref="ILuaStartup"/> instance in use.
    /// </summary>
    private readonly IGlobalEvents _globalEvents;

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="IActionScripts"/> instance in use.
    /// </summary>
    public IActionScripts Actions { get; }

    /// <summary>
    /// A reference to the <see cref="ICreatureEventsScripts"/> instance in use.
    /// </summary>
    public ICreatureEventsScripts CreatureEvents { get; }

    /// <summary>
    /// A reference to the <see cref="IGlobalEventsScripts"/> instance in use.
    /// </summary>
    public IGlobalEventsScripts GlobalEvents { get; }

    /// <summary>
    /// A reference to the <see cref="ITalkActionScripts"/> instance in use.
    /// </summary>
    public ITalkActionScripts TalkActions { get; }

    #endregion

    #region Constructors

    public LuaScriptGameManager(
        ILuaStartup luaStartup,
        IGlobalEvents globalEvents,
        ILogger logger,
        IActionScripts actionsScripts,
        ICreatureEventsScripts creatureEventsScripts,
        IGlobalEventsScripts globalEventsScripts,
        ITalkActionScripts talkActionsScripts)
    {
        _luaStartup = luaStartup;
        _globalEvents = globalEvents;
        _logger = logger;

        Actions = actionsScripts;
        CreatureEvents = creatureEventsScripts;
        TalkActions = talkActionsScripts;
        GlobalEvents = globalEventsScripts;
    }

    #endregion

    #region Public Methods 

    public void Start()
    {
        _luaStartup.Start();
        _globalEvents.Startup();
    }

    #endregion
}
