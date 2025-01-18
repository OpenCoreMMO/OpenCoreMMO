using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaScriptManager : IScriptManager
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
    /// A reference to the <see cref="IActionScriptService"/> instance in use.
    /// </summary>
    public IActionScriptService Actions { get; }

    /// <summary>
    /// A reference to the <see cref="ICreatureEventsScriptService"/> instance in use.
    /// </summary>
    public ICreatureEventsScriptService CreatureEvents { get; }

    /// <summary>
    /// A reference to the <see cref="IGlobalEventsScriptService"/> instance in use.
    /// </summary>
    public IGlobalEventsScriptService GlobalEvents { get; }

    /// <summary>
    /// A reference to the <see cref="IMoveEventsScriptService"/> instance in use.
    /// </summary>
    public IMoveEventsScriptService MoveEvents { get; }

    /// <summary>
    /// A reference to the <see cref="ITalkActionScriptService"/> instance in use.
    /// </summary>
    public ITalkActionScriptService TalkActions { get; }

    #endregion

    #region Constructors

    public LuaScriptManager(
        ILuaStartup luaStartup,
        IGlobalEvents globalEvents,
        ILogger logger,
        IActionScriptService actionsScriptService,
        ICreatureEventsScriptService creatureEventsScriptService,
        IGlobalEventsScriptService globalEventsScriptService,
        IMoveEventsScriptService moveEventsScriptService,
        ITalkActionScriptService talkActionsScriptService)
    {
        _luaStartup = luaStartup;
        _globalEvents = globalEvents;
        _logger = logger;

        Actions = actionsScriptService;
        CreatureEvents = creatureEventsScriptService;
        GlobalEvents = globalEventsScriptService;
        MoveEvents = moveEventsScriptService;
        TalkActions = talkActionsScriptService;
    }

    #endregion

    #region Public Methods 

    public void Initialize()
    {
        _luaStartup.Start();
        _globalEvents.Startup();
    }

    #endregion
}
