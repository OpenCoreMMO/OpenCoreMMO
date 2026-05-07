using System.Linq;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Entities;
using NeoServer.Data.Extensions;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Data.Repositories.Player;

internal static class InventoryManager
{
    public static void SaveBackpack(IPlayer player, NeoContext neoContext)
    {
        if (Guard.AnyNull(player, player.Inventory?.BackpackSlot)) return;

        if (player.Inventory?.BackpackSlot?.Items?.Count == 0) return;

        neoContext.PlayerItems.RemoveRange(neoContext.PlayerItems.Where(x => x.PlayerId == player.Id));

        ContainerManager.Save<PlayerItemEntity>(player, player.Inventory?.BackpackSlot, neoContext);
    }

    public static void SavePlayerInventory(IPlayer player, NeoContext neoContext)
    {
        var playerInventory = neoContext
            .PlayerInventoryItems
            .Where(x => x.PlayerId == player.Id)
            .ToDictionary(x => x.SlotId);

        foreach (var slot in new[]
                 {
                     Slot.Necklace, Slot.Head, Slot.Backpack, Slot.Left, Slot.Body, Slot.Right, Slot.Ring, Slot.Legs,
                     Slot.Ammo, Slot.Feet
                 })
        {
            var item = player.Inventory[slot];

            if (playerInventory.TryGetValue((int)slot, out var playerInventoryItemEntity))
            {
                playerInventoryItemEntity.ServerId = item?.Metadata?.ServerId ?? 0;
                playerInventoryItemEntity.Amount = item?.Amount ?? 0;
                playerInventoryItemEntity.PlayerId = (int)player.Id;
                playerInventoryItemEntity.SlotId = (int)slot;
                playerInventoryItemEntity.Attributes = item.ExtractAllAttributes();

                neoContext.PlayerInventoryItems.Update(playerInventoryItemEntity);
                continue;
            }

            neoContext.PlayerInventoryItems.Add(new PlayerInventoryItemEntity
            {
                Amount = item?.Amount ?? 0,
                PlayerId = (int)player.Id,
                SlotId = (int)slot,
                ServerId = item?.Metadata?.ServerId ?? 0,
                Attributes = item.ExtractAllAttributes()
            });
        }
    }
}