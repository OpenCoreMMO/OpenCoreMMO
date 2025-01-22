using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaActionScriptService : IActionScriptService
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="IActions"/> instance in use.
    /// </summary>
    private readonly IActions _actions;

    #endregion

    #region Constructors

    public LuaActionScriptService(
        ILogger logger,
        IActions actions)
    {
        _logger = logger;

        _actions = actions;
    }

    #endregion

    #region Public Methods 

    public bool HasAction(IItem item) => _actions.GetAction(item) != null;

    public bool UseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item, IThing target = null)
    {
        return UseItem(player, pos, pos, stackpos, item, target, false);
    }

    public bool UseItem(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, IThing target = null, bool isHotkey = false)
    {
        var action = _actions.GetAction(item);

        if (target != null)
        {
            if (target is ITile tile)
            {
                target = tile.TopItemOnStack;
            }
            else if (target is ICreature creature)
            {
                toPos = creature.Location;
                toStackPos = creature.Tile.GetCreatureStackPositionIndex(player);
            }
        }

        if (action != null)
            return action.ExecuteUse(
                player,
                item,
                fromPos,
                target,
                toPos,
                isHotkey);
        else
            _logger.Warning("Action with item id {ItemServerId} has not found into LuaJIT Scripts.", item.ServerId);

        return false;
    }

    #endregion
}
