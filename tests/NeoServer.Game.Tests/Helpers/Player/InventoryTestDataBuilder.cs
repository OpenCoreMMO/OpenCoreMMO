using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Creatures.Player.Inventory;

namespace NeoServer.Game.Tests.Helpers.Player;

public static class InventoryTestDataBuilder
{
    public static IInventory Build(IPlayer player = null,
        Dictionary<Slot, (IItem Item, ushort Id)> inventoryMap = null, ICoinTypeStore coinTypeStore = null)
    {
        player ??= PlayerTestDataBuilder.Build();
        inventoryMap ??= new Dictionary<Slot, (IItem Item, ushort Id)>();

        return new Inventory(player, inventoryMap);
    }

    public static Dictionary<Slot, (IItem Item, ushort Id)> GenerateInventory()
    {
        return new Dictionary<Slot, (IItem Item, ushort Id)>
        {
            [Slot.Backpack] = new(ItemTestData.CreateBackpack(), 1),
            [Slot.Ammo] = new(ItemTestData.CreateAmmo(2, 10), 2),
            [Slot.Head] = new(ItemTestData.CreateBodyEquipmentItem(3, "head"), 3),
            [Slot.Left] = new(ItemTestData.CreateWeaponItem(4, "axe"), 4),
            [Slot.Body] = new(ItemTestData.CreateBodyEquipmentItem(5, "body"), 5),
            [Slot.Feet] = new(ItemTestData.CreateBodyEquipmentItem(6, "feet"), 6),
            [Slot.Right] = new(ItemTestData.CreateBodyEquipmentItem(7, "", "shield"), 7),
            [Slot.Ring] =
                new(ItemTestData.CreateDefenseEquipmentItem(8, "ring"), 8),
            [Slot.Necklace] =
                new(ItemTestData.CreateDefenseEquipmentItem(10, "necklace"),
                    10),
            [Slot.Legs] = new(ItemTestData.CreateBodyEquipmentItem(11, "legs"), 11)
        };
    }
}