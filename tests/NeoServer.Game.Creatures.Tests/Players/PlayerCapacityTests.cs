using System.Collections.Generic;
using FluentAssertions;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players;

public class PlayerCapacityTests
{
    public static IEnumerable<object[]> SlotItemsData =>
        new List<object[]>
        {
            new object[]
            {
                Slot.Backpack, ItemTestData.CreateBackpack(105, 20, new List<IItem>
                {
                    ItemTestData.CreateWeaponItem(100, weight: 10),
                    ItemTestData.CreateWeaponItem(101, weight: 10),
                    ItemTestData.CreateCumulativeItem(101, weight: 1, amount: 20),
                    ItemTestData.CreateBackpack(105, 20, new List<IItem>
                    {
                        ItemTestData.CreateWeaponItem(101, weight: 10),
                        ItemTestData.CreateCumulativeItem(101, weight: 1, amount: 10)
                    })
                })
            },
            new object[] { Slot.Ammo, ItemTestData.CreateAmmo(100, 100, weight: 1) },
            new object[] { Slot.Head, ItemTestData.CreateBodyEquipmentItem(100, "head", weight: 100) },
            new object[] { Slot.Left, ItemTestData.CreateWeaponItem(100, "axe", weight: 100) },
            new object[] { Slot.Body, ItemTestData.CreateBodyEquipmentItem(100, "body", weight: 100) },
            new object[] { Slot.Feet, ItemTestData.CreateBodyEquipmentItem(100, "feet", weight: 100) },
            new object[] { Slot.Right, ItemTestData.CreateBodyEquipmentItem(100, "", "shield", 100) },
            new object[] { Slot.Ring, ItemTestData.CreateBodyEquipmentItem(100, "ring", weight: 100) },
            new object[]
                { Slot.Left, ItemTestData.CreateWeaponItem(100, weaponType: "sword", twoHanded: true, weight: 100) },
            new object[] { Slot.Necklace, ItemTestData.CreateBodyEquipmentItem(100, "necklace", weight: 100) }
        };

    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void Player_capacity_decreases_when_item_added_to_inventory(Slot slot, IItem item)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        //act
        player.Inventory.AddItem(item, slot);

        //assert
        player.FreeCapacity.Should().Be(900);
    }

    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void Player_capacity_increases_when_item_removed_to_inventory(Slot slot, IItem item)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        player.Inventory.AddItem(item, slot);

        //act
        player.Inventory.RemoveItem(slot, item.Amount);

        //assert
        player.FreeCapacity.Should().Be(1000);
    }

    [Fact]
    public void Player_capacity_increases_when_item_removed_from_equipped_backpack()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var backpack = ItemTestData.CreateBackpack(weight: 20);

        var item = ItemTestData.CreateWeaponItem(100, weight: 20);
        var item2 = ItemTestData.CreateCumulativeItem(101, 20, weight: 1);
        var item3 = ItemTestData.CreateFood(102, 20, 1);

        var innerBag = ItemTestData.CreateBackpack(weight: 20);

        var item4 = ItemTestData.CreateFood(102, 20, 1);
        var item5 = ItemTestData.CreateCumulativeItem(101, 20, weight: 1);
        var item6 = ItemTestData.CreateWeaponItem(100, weight: 20);

        var item7 = ItemTestData.CreateCumulativeItem(103, 20, weight: 1);
        var item8 = ItemTestData.CreateWeaponItem(100, weight: 20);

        backpack.AddItem(item);
        backpack.AddItem(item2);
        backpack.AddItem(item3);
        backpack.AddItem(innerBag);

        innerBag.AddItem(item4);
        innerBag.AddItem(item5);
        innerBag.AddItem(item6);
        innerBag.AddItem(item7);
        innerBag.AddItem(item8);

        player.Inventory.AddItem(backpack, Slot.Backpack);

        //act / assert
        item4.Reduce(10);
        player.FreeCapacity.Should().Be(810);

        item4.Reduce(10);
        player.FreeCapacity.Should().Be(820);

        item5.Reduce(10);
        player.FreeCapacity.Should().Be(830);

        item5.Reduce(10);
        player.FreeCapacity.Should().Be(840);

        innerBag.RemoveItem(item6, 1);
        player.FreeCapacity.Should().Be(860);

        //Remove from root backpack

        item3.Reduce(10);
        player.FreeCapacity.Should().Be(870);

        item3.Reduce(10);
        player.FreeCapacity.Should().Be(880);

        item2.Reduce(10);
        player.FreeCapacity.Should().Be(890);

        item2.Reduce(10);
        player.FreeCapacity.Should().Be(900);

        backpack.RemoveItem(item, 1);
        player.FreeCapacity.Should().Be(920);

        //remove from inner bag again
        innerBag.RemoveItem(item7, 20);
        player.FreeCapacity.Should().Be(940);

        //remove entire inner bag
        backpack.RemoveItem(innerBag, 1);
        player.FreeCapacity.Should().Be(980);

        player.Inventory.RemoveItem(Slot.Backpack, 1);
    }

    [Fact]
    public void Player_capacity_decreases_when_item_added_to_equipped_backpack()
    {
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var backpack = ItemTestData.CreateBackpack(weight: 20);

        player.Inventory.AddItem(backpack);
        player.FreeCapacity.Should().Be(980);

        var item = ItemTestData.CreateWeaponItem(100, weight: 20);
        backpack.AddItem(item);
        player.FreeCapacity.Should().Be(960);

        var item2 = ItemTestData.CreateCumulativeItem(101, 10, weight: 1);
        backpack.AddItem(item2);
        player.FreeCapacity.Should().Be(950);

        var item2B = ItemTestData.CreateCumulativeItem(101, 10, weight: 1);
        backpack.AddItem(item2B);
        player.FreeCapacity.Should().Be(940);

        var item3 = ItemTestData.CreateFood(102, 10, 1);
        backpack.AddItem(item3);
        player.FreeCapacity.Should().Be(930);

        var item3B = ItemTestData.CreateFood(102, 10, 1);
        backpack.AddItem(item3B);
        player.FreeCapacity.Should().Be(920);

        var innerBag = ItemTestData.CreateBackpack(weight: 20);
        var item8 = ItemTestData.CreateWeaponItem(100, weight: 20);
        innerBag.AddItem(item8);

        backpack.AddItem(innerBag);
        player.FreeCapacity.Should().Be(880);

        var item4 = ItemTestData.CreateFood(102, 10, 1);
        innerBag.AddItem(item4);
        player.FreeCapacity.Should().Be(870);

        var item4B = ItemTestData.CreateFood(102, 10, 1);
        innerBag.AddItem(item4B);
        player.FreeCapacity.Should().Be(860);

        var item5 = ItemTestData.CreateCumulativeItem(101, 10, weight: 1);
        innerBag.AddItem(item5);
        player.FreeCapacity.Should().Be(850);

        var item5B = ItemTestData.CreateCumulativeItem(101, 10, weight: 1);
        innerBag.AddItem(item5B);
        player.FreeCapacity.Should().Be(840);

        var item6 = ItemTestData.CreateWeaponItem(100, weight: 20);
        innerBag.AddItem(item6);
        player.FreeCapacity.Should().Be(820);

        var item7 = ItemTestData.CreateCumulativeItem(103, 20, weight: 1);
        innerBag.AddItem(item7);
        player.FreeCapacity.Should().Be(800);
    }
}