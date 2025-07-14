using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Common.Texts;
using NeoServer.Domain.World.Algorithms;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World.Services;

public class ToMapMovementService : IToMapMovementService
{
    private readonly IItemMovementService _itemMovementService;
    private readonly IMap _map;
    private readonly IMapService _mapService;

    public ToMapMovementService(IMap map, IMapService mapService, IItemMovementService itemMovementService)
    {
        _map = map;
        _mapService = mapService;
        _itemMovementService = itemMovementService;
    }

    public void Move(IPlayer player, MovementParams itemThrow)
    {
        var finalTile = _mapService.GetFinalTile(itemThrow.ToLocation);

        if (finalTile is not IDynamicTile)
        {
            OperationFailService.Send(player.CreatureId, TextConstants.NOT_ENOUGH_ROOM);
            return;
        }

        if (!SightClear.IsSightClear(_map, player.Location, itemThrow.ToLocation, false))
        {
            OperationFailService.Send(player.CreatureId, TextConstants.YOU_CANNOT_THROW_THERE);
            return;
        }

        FromGround(player, itemThrow);
        FromInventory(player, itemThrow);
        FromContainer(player, itemThrow);
    }

    private void FromGround(IPlayer player, MovementParams movementParams)
    {
        if (movementParams.FromLocation.Type != LocationType.Ground) return;

        if (_map[movementParams.FromLocation] is not DynamicTile fromTile) return;
        if (_map[movementParams.ToLocation] is not DynamicTile toTile) return;

        if (fromTile.TopDownItemOnStack is not { } item) return;

        var finalTile = (DynamicTile)_mapService.GetFinalTile(toTile.Location);

        _itemMovementService.Move(player, item, fromTile, finalTile, movementParams.Amount, 0, 0);
    }

    private void FromInventory(IPlayer player, MovementParams movementParams)
    {
        if (movementParams.FromLocation.Type is not LocationType.Slot) return;
        if (_map[movementParams.ToLocation] is not IDynamicTile toTile) return;

        var item = player.Inventory[movementParams.FromLocation.Slot];
        var itemIsPickupable = item?.IsPickupable ?? false;
        if (!itemIsPickupable) return;

        var finalTile = (DynamicTile)_mapService.GetFinalTile(toTile.Location);

        player.MoveItem(item, player.Inventory, finalTile, movementParams.Amount,
            (byte)movementParams.FromLocation.Slot, 0);
    }

    private void FromContainer(IPlayer player, MovementParams itemThrow)
    {
        if (itemThrow.FromLocation.Type is not LocationType.Container) return;
        if (_map[itemThrow.ToLocation] is not IDynamicTile toTile) return;

        var container = player.Containers[itemThrow.FromLocation.ContainerId];
        var item = container[itemThrow.FromLocation.ContainerSlot];
        var itemIsPickupable = item?.IsPickupable ?? false;

        if (!itemIsPickupable) return;

        var finalTile = (DynamicTile)_mapService.GetFinalTile(toTile.Location);

        _itemMovementService.Move(player, item, container, finalTile, itemThrow.Amount,
            (byte)itemThrow.FromLocation.ContainerSlot, 0);
    }
}