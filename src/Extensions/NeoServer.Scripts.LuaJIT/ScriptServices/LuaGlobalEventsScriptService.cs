using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaGlobalEventsScriptService : IGlobalEventsScriptService
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="IGlobalEvents"/> instance in use.
    /// </summary>
    private readonly IGlobalEvents _globalEvents;

    #endregion

    #region Constructors

    public LuaGlobalEventsScriptService(
        ILogger logger,
        IGlobalEvents globalEvents)
    {
        _logger = logger;
        _globalEvents = globalEvents;
    }

    #endregion

    #region Public Methods 

    public void ExecuteRecord(int current, int old)
    {
        foreach (var (key, globalEvent) in _globalEvents.GetEventMap(GlobalEventType.GLOBALEVENT_RECORD))
            globalEvent.ExecuteRecord(current, old);
    }

    public void ExecuteShutdown()
        => _globalEvents.Shutdown();

    public void ExecuteSave()
        => _globalEvents.Save();

    #endregion
}
