using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaMoveEventsScriptService : IMoveEventsScriptService
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

    public LuaMoveEventsScriptService(
        ILogger logger,
        IMoveEvents moveEvents)
    {
        _logger = logger;

        _moveEvents = moveEvents;
    }

    #endregion

    #region Public Methods 

    public void CreatureMove(ICreature creature, Location fromLocation, Location toLocation)
    {
        _moveEvents.OnCreatureMove(creature, fromLocation, toLocation);
    }

    public void ItemMove(IItem item, ITile tile, bool isAdd)
    {
        _moveEvents.OnItemMove(item, tile, isAdd);
    }

    #endregion
}
