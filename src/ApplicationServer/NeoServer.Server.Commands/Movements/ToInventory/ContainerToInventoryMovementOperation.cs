using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Movements.ToInventory;

public class ContainerToInventoryMovementOperation
{
    public static void Execute(IPlayer player, ItemThrowPacket itemThrow)
    {
        var container = player.Containers[itemThrow.FromLocation.ContainerId];

        var item = container[itemThrow.FromLocation.ContainerSlot];

        if (item is null) return;

        if (!item.IsPickupable) return;

        player.MoveItem(item, container, player.Inventory, itemThrow.Count,
            (byte)itemThrow.FromLocation.ContainerSlot, (byte)itemThrow.ToLocation.Slot);
    }

    public static bool IsApplicable(ItemThrowPacket itemThrowPacket)
    {
        return itemThrowPacket.FromLocation.Type == LocationType.Container
               && itemThrowPacket.ToLocation.Type == LocationType.Slot;
    }
}