using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Commands.Movements.ToContainer;

public class MapToContainerMovementOperation
{
    private readonly IItemMovementService _itemMovementService;

    public MapToContainerMovementOperation(IItemMovementService itemMovementService)
    {
        _itemMovementService = itemMovementService;
    }

    public void Execute(IPlayer player, IGameServer game, IMap map, ItemThrowPacket itemThrow)
    {
        MapToContainer(player, map, itemThrow);
    }

    private void MapToContainer(IPlayer player, IMap map, ItemThrowPacket itemThrow)
    {
        var tile = map[itemThrow.FromLocation];

        if (tile is not IDynamicTile fromTile) return;
        var item = fromTile.TopItemOnStack;

        if (item is null) return;
        if (!item.IsPickupable) return;

        var container = player.Containers[itemThrow.ToLocation.ContainerId];
        if (container is null) return;

        _itemMovementService.Move(player, item, fromTile, container,
            itemThrow.Count, 0, (byte)itemThrow.ToLocation.ContainerSlot);
    }

    public static bool IsApplicable(ItemThrowPacket itemThrowPacket)
    {
        return itemThrowPacket.FromLocation.Type == LocationType.Ground
               && itemThrowPacket.ToLocation.Type == LocationType.Container;
    }
}