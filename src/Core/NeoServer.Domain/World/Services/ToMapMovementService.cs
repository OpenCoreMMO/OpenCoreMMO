using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World.Services;

public class ToMapMovementService(
    IMap map,
    IItemMovementService itemMovementService,
    ICreaturePushService creaturePushService,
    IMapItemMovementService mapItemMovementService)
    : IToMapMovementService
{
    public void Move(IPlayer player, MovementParams itemThrow)
    {
        FromGround(player, itemThrow);
        FromInventory(player, itemThrow);
        FromContainer(player, itemThrow);
    }

    private void FromGround(IPlayer player, MovementParams movementParams)
    {
        if (movementParams.FromLocation.Type != LocationType.Ground) return;

        if (map[movementParams.FromLocation] is not DynamicTile fromTile) return;
        if (map[movementParams.ToLocation] is not { } toTile) return;

        // Move item if present, otherwise push creature if present
        if (fromTile.TopDownItemOnStack is { CanBeMoved: true } item)
        {
            mapItemMovementService.Move(player, item, fromTile, toTile, movementParams.Amount, 0, 0);
            return;
        }

        if (fromTile.TopCreatureOnStack is { } creature && !ReferenceEquals(creature, player))
        {
            _ = (DynamicTile)map.GetTileDestination(toTile.Location);
            creaturePushService.PushCreature(player, creature, toTile);
        }
    }

    private void FromInventory(IPlayer player, MovementParams movementParams)
    {
        if (movementParams.FromLocation.Type is not LocationType.Slot) return;
        if (map[movementParams.ToLocation] is not IDynamicTile toTile) return;

        var item = player.Inventory[movementParams.FromLocation.Slot];
        var itemIsPickupable = item?.IsPickupable ?? false;
        if (!itemIsPickupable) return;

        var finalTile = (DynamicTile)map.GetTileDestination(toTile.Location);

        itemMovementService.Move(player, item, player.Inventory, finalTile,  movementParams.Amount,
             (byte)movementParams.FromLocation.Slot, 0);
        
    }

    private void FromContainer(IPlayer player, MovementParams itemThrow)
    {
        if (itemThrow.FromLocation.Type is not LocationType.Container) return;
        if (map[itemThrow.ToLocation] is not IDynamicTile toTile) return;

        var container = player.Containers[itemThrow.FromLocation.ContainerId];
        var item = container[itemThrow.FromLocation.ContainerSlot];
        var itemIsPickupable = item?.IsPickupable ?? false;

        if (!itemIsPickupable) return;

        var finalTile = (DynamicTile)map.GetTileDestination(toTile.Location);

        itemMovementService.Move(player, item, container, finalTile, itemThrow.Amount,
             (byte)itemThrow.FromLocation.ContainerSlot, 0);
    }
}