using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeoServer.Data.Entities;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Data.Seeds;

internal static class PlayerInventorySeed
{
    public static void Seed(EntityTypeBuilder<PlayerInventoryItemEntity> builder)
    {
        var id = 0;
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Head, 8820);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Backpack, 1988);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Body, 8819);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Right, 2175);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Left, 2190);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Legs, 2649);
        SeedPlayerInventory(builder, --id, 2, (int)Slot.Feet, 2643);

        SeedPlayerInventory(builder, --id, 4, (int)Slot.Head, 8820);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Backpack, 1988);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Body, 8819);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Right, 2175);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Left, 2182);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Legs, 2649);
        SeedPlayerInventory(builder, --id, 4, (int)Slot.Feet, 2643);

        SeedPlayerInventory(builder, --id, 3, (int)Slot.Head, 2457);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Backpack, 1988);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Body, 2465);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Right, 2509);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Left, 8602);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Legs, 2478);
        SeedPlayerInventory(builder, --id, 3, (int)Slot.Feet, 2643);

        SeedPlayerInventory(builder, --id, 5, (int)Slot.Head, 2461);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Backpack, 1988);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Body, 2660);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Left, 2456);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Legs, 8923);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Feet, 2643);
        SeedPlayerInventory(builder, --id, 5, (int)Slot.Ammo, 2544);

        SeedPlayerInventory(builder, --id, 1, (int)Slot.Head, 2457);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Necklace, 2173);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Backpack, 1988);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Body, 2463);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Right, 2516);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Left, 2383);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Legs, 2647);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Feet, 2643);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Ring, 2209);
        SeedPlayerInventory(builder, --id, 1, (int)Slot.Ammo, 2543);
    }

    private static void SeedPlayerInventory(EntityTypeBuilder<PlayerInventoryItemEntity> builder, int id, int playerId,
        int slotId, int serverId)
    {
        builder.HasData(new PlayerInventoryItemEntity
        {
            Id = id,
            PlayerId = playerId,
            SlotId = slotId,
            ServerId = serverId,
            Amount = 1
        });
    }
}