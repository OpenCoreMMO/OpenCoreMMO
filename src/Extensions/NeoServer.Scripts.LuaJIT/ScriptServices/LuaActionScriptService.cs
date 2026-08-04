using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Houses;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.ScriptServices;

public class LuaActionScriptService : IActionScriptService
{
    #region Constructors

    public LuaActionScriptService(
        ILogger logger,
        IActions actions,
        IHouseStore houseStore,
        IMap map)
    {
        _logger = logger;
        _actions = actions;
        _houseStore = houseStore;
        _map = map;
    }

    #endregion

    #region Dependency Injections

    /// <summary>
    ///     A reference to the <see cref="ILogger" /> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///     A reference to the <see cref="IActions" /> instance in use.
    /// </summary>
    private readonly IActions _actions;

    /// <summary>
    ///     A reference to the <see cref="IHouseStore" /> instance in use.
    /// </summary>
    private readonly IHouseStore _houseStore;

    /// <summary>
    ///     A reference to the <see cref="IMap" /> instance in use.
    /// </summary>
    private readonly IMap _map;

    #endregion

    #region Public Methods

    public bool HasAction(IItem item)
    {
        if (item is null) return false;
        return _actions.GetAction(item) != null;
    }

    public bool UseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item, IThing target = null)
    {
        if (item is null) return false;

        return UseItem(player, pos, pos, stackpos, item, target);
    }

    public bool UseItem(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item,
        IThing target = null, bool isHotkey = false)
    {
        var action = _actions.GetAction(item);

        if (target != null)
        {
            if (target is ITile tile)
            {
                target = tile.TopDownItemOnStack;
            }
            else if (target is ICreature creature)
            {
                toPos = creature.Location;
                toStackPos = creature.Tile.GetCreatureStackPositionIndex(player);
            }
        }

        if (action is null)
        {
            _logger.Warning("Action with item id {ItemServerId} has not found into LuaJIT Scripts", item.ServerId);
            return false;
        }

        if (!CanUseHouseDoor(player, item))
        {
            OperationFailService.Send(player, InvalidOperation.NotInvited);
            return false;
        }

        return action.ExecuteUse(
            player,
            item,
            fromPos,
            target,
            toPos,
            isHotkey);
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     House door access gate. Non-house doors are always allowed.
    ///     House doors without a DoorId fail closed for guests/strangers (owners/subowners/GM still allowed).
    /// </summary>
    private bool CanUseHouseDoor(IPlayer player, IItem item)
    {
        if (!item.IsDoor) return true;

        var tile = item.Parent as ITile ?? item.Owner as ITile ?? _map.GetTile(item.Location);
        if (tile is null) return true;

        var house = _houseStore.GetByTile(tile);
        if (house is null) return true;

        if (!TryGetDoorId(item, out var doorId))
        {
            _logger.Warning(
                "House door at {Location} (item {ItemServerId}) has no DoorId; denying use for non-subowners",
                item.Location,
                item.ServerId);
            return house.GetAccessLevel(player) >= HouseAccessLevel.SubOwner;
        }

        return house.CanUseDoor(player, doorId);
    }

    private static bool TryGetDoorId(IItem item, out uint doorId)
    {
        doorId = 0;
        if (item.Attributes is null) return false;

        if (item.Attributes.TryGetAttribute<byte>(ItemAttribute.DoorId, out var doorIdByte))
        {
            doorId = doorIdByte;
            return true;
        }

        if (item.Attributes.TryGetAttribute(ItemAttribute.DoorId, out string doorIdStr) &&
            uint.TryParse(doorIdStr, out doorId))
        {
            return true;
        }

        return false;
    }

    #endregion
}
