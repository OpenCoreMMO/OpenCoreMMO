using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Networking.Packets.Incoming;

namespace NeoServer.Server.Commands.Movements.ToInventory;

public class InventoryToInventoryOperation
{
    public static void Execute(IPlayer player, ItemThrowPacket itemThrow)
    {
        var item = player.Inventory[itemThrow.FromLocation.Slot];
        if (item is null) return;

        if (!item.IsPickupable) return;

        player.MoveItem(item, player.Inventory, player.Inventory, itemThrow.Count,
            (byte)itemThrow.FromLocation.Slot, (byte)itemThrow.ToLocation.Slot);
    }

    public static bool IsApplicable(ItemThrowPacket itemThrowPacket)
    {
        return itemThrowPacket.FromLocation.Type == LocationType.Slot
               && itemThrowPacket.ToLocation.Type == LocationType.Slot;
    }
}