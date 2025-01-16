using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Services;

public class LuaMoveEventsScripts : IMoveEventsScripts
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="IMoveEvents"/> instance in use.
    /// </summary>
    private readonly IMoveEvents _moveEvents;

    #endregion

    #region Constructors

    public LuaMoveEventsScripts(
        ILogger logger,
        IMoveEvents moveEvents)
    {
        _logger = logger;

        _moveEvents = moveEvents;
    }

    #endregion

    #region Public Methods 

    #endregion
}
