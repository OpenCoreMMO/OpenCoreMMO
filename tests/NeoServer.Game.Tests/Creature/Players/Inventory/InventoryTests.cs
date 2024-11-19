using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Item.Items.Weapons;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Tests.Creature.Players.Inventory;

public class InventoryTests
{
    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void Item_is_added_to_inventory_when_slot_is_empty(Slot slot, IItem item)
    {
        //arrange
        var sut = InventoryTestDataBuilder.Build();

        //act
        sut.AddItem(item, slot);

        //assert
        item.Should().Be(sut[slot]);
    }

    [Theory]
    [MemberData(nameof(WrongSlotItemsData))]
    public void Item_is_not_added_to_inventory_when_slot_is_wrong(Slot slot, IItem item)
    {
        //arrange
        var sut = InventoryTestDataBuilder.Build(inventoryMap: new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.AddItem(item, slot);

        //assert
        result.Failed.Should().BeTrue();
        result.Error.Should().Be(InvalidOperation.CannotDress);
    }

    [Fact]
    public void TwoHanded_Weapon_cannot_be_added_when_inventory_has_shield()
    {
        //arrange
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var twoHanded = ItemTestData.CreateWeaponItem(100, weaponType: "axe", twoHanded: true);

        //act
        var result = sut.AddItem(twoHanded, Slot.Left);

        //assert
        twoHanded.Should().BeSameAs(sut[Slot.Left]);
        sut[Slot.Right].Should().BeNull();
        result.Succeeded.Should().BeTrue();

        //act
        var shield = ItemTestData.CreateBodyEquipmentItem(101, "", "shield");

        //assert
        twoHanded.Should().BeSameAs(sut[Slot.Left]);

        //act
        result = sut.AddItem(shield, Slot.Right);

        //assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(InvalidOperation.BothHandsNeedToBeFree);

        sut[Slot.Right].Should().BeNull();
    }

    [Fact]
    public void AddItemToSlot_Add_Shield_And_TwoHanded_Returns_False()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var shield = ItemTestData.CreateBodyEquipmentItem(101, "", "shield");

        var result = sut.AddItem(shield, Slot.Right);

        Assert.Same(shield, sut[Slot.Right]);
        Assert.Null(sut[Slot.Left]);
        Assert.True(result.Succeeded);

        var twoHanded = ItemTestData.CreateWeaponItem(100, weaponType: "axe", twoHanded: true);
        Assert.Same(shield, sut[Slot.Right]);

        result = sut.AddItem(twoHanded, Slot.Left);
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.BothHandsNeedToBeFree, result.Error);

        Assert.Null(sut[Slot.Left]);
    }

    [Fact]
    public void Item_cannot_be_added_when_exceeds_player_capacity()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var feet = ItemTestData.CreateBodyEquipmentItem(101, "feet");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        //act
        sut.AddItem(legs, Slot.Legs);
        var result = sut.AddItem(feet, Slot.Feet);

        //assert
        result.Succeeded.Should().BeTrue();

        //act
        result = sut.AddItem(body, Slot.Body);

        //assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(InvalidOperation.TooHeavy);
    }

    [Fact]
    public void Item_cannot_be_added_to_inventory_backpack_when_exceeds_capacity()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 130);
        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        sut.AddItem(legs, Slot.Legs);
        sut.AddItem(body, Slot.Body);

        var bp = ItemTestData.CreateBackpack();
        var bp2 = ItemTestData.CreateBackpack();

        bp.AddItem(bp2);

        //act
        var result = sut.AddItem(bp, Slot.Backpack);

        //assert
        result.Succeeded.Should().BeTrue();

        //act
        result = sut.AddItem(ItemTestData.CreateAmmo(105, 20), Slot.Backpack);

        //assert
        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(InvalidOperation.TooHeavy);
    }

    [Fact]
    public void Item_goes_to_backpack_when_inventory_has_a_backpack()
    {
        //arrange
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 200),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        sut.AddItem(legs, Slot.Legs);
        sut.AddItem(body, Slot.Body);

        var bp = ItemTestData.CreateBackpack();
        var bp2 = ItemTestData.CreateBackpack();

        bp.AddItem(bp2);

        //act
        var result = sut.AddItem(bp, Slot.Backpack);

        //assert
        result.Succeeded.Should().BeTrue();

        //act
        sut.AddItem(ItemTestData.CreateAmmo(105, 20), Slot.Backpack);

        //assert
        ((IContainer)sut[Slot.Backpack])[0].ClientId.Should().Be(105);
    }

    [Fact]
    public void Inventory_weight_returns_a_sum_of_all_items_weight()
    {
        //arrange
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 2000),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");
        var feet = ItemTestData.CreateBodyEquipmentItem(100, "feet");
        var head = ItemTestData.CreateBodyEquipmentItem(100, "head");
        var necklace = ItemTestData.CreateBodyEquipmentItem(100, "necklace");
        var ring = ItemTestData.CreateBodyEquipmentItem(100, "ring");
        var shield = ItemTestData.CreateBodyEquipmentItem(100, "shield", "shield");
        var ammo = ItemTestData.CreateAmmo(100, 100);
        var weapon = ItemTestData.CreateWeaponItem(100, "club");
        var weapon2 = ItemTestData.CreateWeaponItem(101, "club");

        sut.AddItem(legs, Slot.Legs);
        sut.AddItem(body, Slot.Body);
        sut.AddItem(feet, Slot.Feet);
        sut.AddItem(head, Slot.Head);
        sut.AddItem(necklace, Slot.Necklace);
        sut.AddItem(ring, Slot.Ring);
        sut.AddItem(shield, Slot.Right);
        sut.AddItem(weapon, Slot.Left);
        sut.AddItem(ammo, Slot.Ammo);

        var container = ItemTestData.CreateBackpack();
        container.AddItem(weapon2);

        sut.AddItem(container, Slot.Backpack);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 60));

        //assert
        sut.TotalWeight.Should().Be(540);
    }

    [Theory]
    [MemberData(nameof(SlotSwapItemsData))]
    public void Item_is_swapped_from_slot_on_adding_new_item_to_same_slot(Slot slot, IItem item,
        IItem newItem)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.AddItem(item, slot);

        //assert
        result.Value.HasAnyOperation.Should().BeFalse();

        //act
        result = sut.AddItem(newItem, slot);

        //assert
        newItem.Should().BeSameAs(sut[slot]);
        item.Should().BeSameAs(result.Value.Operations[0].Item1);
    }

    [Theory]
    [MemberData(nameof(SlotJoinItemsData))]
    public void Cumulative_item_joins_when_has_same_item_type_in_the_slot(Slot slot, ICumulative item,
        ICumulative newItem, ICumulative resultItem)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.AddItem(item, slot);

        //assert
        result.Value.HasAnyOperation.Should().BeFalse();

        //act
        sut.AddItem(newItem, slot);

        //assert
        sut[slot].Should().Be(item);
        ((ICumulative)sut[slot]).Amount.Should().Be(resultItem.Amount);
    }

    [Fact]
    public void Cumulative_item_added_raises_reduced_event()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;

        //act
        sut.AddItem(item, Slot.Ammo);

        var eventRaised = false;
        var itemRemovedEventRaised = false;

        item.OnReduced += (_, _) => eventRaised = true;
        sut.OnItemRemovedFromSlot += (_, _, _, _) =>
            itemRemovedEventRaised = true;

        item.Throw();

        //assert
        eventRaised.Should().BeTrue();
        itemRemovedEventRaised.Should().BeTrue();
    }

    [Fact]
    public void Cumulative_joined_item_raises_reduced_event()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var sut = InventoryTestDataBuilder.Build(player,
            new Dictionary<Slot, (IItem Item, ushort Id)>());
        var initialItem = ItemTestData.CreateAmmo(101, 1) as Ammo;
        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;

        //act
        sut.AddItem(initialItem, Slot.Ammo);
        sut.AddItem(item, Slot.Ammo);

        var eventRaised = false;
        var itemRemovedEventRaised = false;

        item.OnReduced += (_, _) => eventRaised = true;
        sut.OnItemRemovedFromSlot += (_, _, _, _) =>
            itemRemovedEventRaised = true;

        item.Throw();

        //assert
        eventRaised.Should().BeTrue();
        itemRemovedEventRaised.Should().BeTrue();
    }

    [Fact]
    public void Cumulative_swapped_item_do_not_raise_reduced_event()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);

        var sut = InventoryTestDataBuilder.Build(player, new Dictionary<Slot, (IItem Item, ushort Id)>());
        var initialItem = ItemTestData.CreateAmmo(101, 3) as Ammo;
        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;

        //act
        sut.AddItem(initialItem, Slot.Ammo);
        var result = sut.AddItem(item, Slot.Ammo);

        var itemRemovedEventRaised = false;

        var swapped = result.Value.Operations[0].Item1 as Ammo;

        sut.OnItemRemovedFromSlot += (_, _, _, _) => itemRemovedEventRaised = true;

        swapped.Throw();

        //assert
        itemRemovedEventRaised.Should().BeFalse();
    }

    [Fact]
    public void
        Cumulative_item_joins_item_on_slot_and_returns_exceeding_amount_when_slot_has_same_cumulative_item_type()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player, new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.AddItem(ItemTestData.CreateAmmo(100, 50), Slot.Ammo);

        //assert
        result.Value.HasAnyOperation.Should().BeFalse();

        //act
        result = sut.AddItem(ItemTestData.CreateAmmo(100, 80), Slot.Ammo);

        //assert
        ((ICumulative)result.Value.Operations[0].Item1).Amount.Should().Be(30);
    }

    [Theory]
    [MemberData(nameof(BackpackSlotAddItemsData))]
    public void Item_is_added_to_backpack_when_backpack_slot_has_a_backpack(IItem item)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player, new Dictionary<Slot, (IItem Item, ushort Id)>());
        var backpack = ItemTestData.CreateBackpack();

        sut.AddItem(backpack, Slot.Backpack);

        //act
        var result = sut.AddItem(item, Slot.Backpack);

        //assert
        result.Value.HasAnyOperation.Should().BeFalse();

        sut[Slot.Backpack].Should().Be(backpack);
        backpack.SlotsUsed.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void Item_location_changes_when_added_to_inventory(Slot slot, IItem item)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player, new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        sut.AddItem(item, slot);

        //assert
        Location.Inventory(slot).X.Should().Be(item.Location.X);
        Location.Inventory(slot).Y.Should().Be(item.Location.Y);
        Location.Inventory(slot).Z.Should().Be(item.Location.Z);
    }

    [Fact]
    public void Non_dressable_item_cannot_be_added_to_inventory()
    {
        //arrange
        var item = ItemTestData.CreateRegularItem(1);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.CanAddItem(item.Metadata);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Be(InvalidOperation.NotEnoughRoom);
    }

    [InlineData("head")]
    [InlineData("body")]
    [InlineData("weapon")]
    [InlineData("shield")]
    [InlineData("necklace")]
    [InlineData("ring")]
    [InlineData("backpack")]
    [InlineData("feet")]
    [InlineData("ammo")]
    [Theory]
    public void Item_can_be_added_when_slot_is_empty(string bodyPosition)
    {
        //arrange
        var item = ItemTestData.CreateBodyEquipmentItem(1, bodyPosition);
        var player = PlayerTestDataBuilder.Build(capacity: 1000);
        var sut = InventoryTestDataBuilder.Build(player, new Dictionary<Slot, (IItem Item, ushort Id)>());

        //act
        var result = sut.CanAddItem(item.Metadata);

        //assert
        result.Succeeded.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public void CanAddItem_When_Slot_Is_Not_Empty_And_Adding_Regular_Item_Returns_Not_Enough_Room()
    {
        var bodyItem = ItemTestData.CreateBodyEquipmentItem(1, "body");
        var weapon = ItemTestData.CreateBodyEquipmentItem(3, "weapon");

        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Body, (bodyItem, 1) },
                { Slot.Left, (weapon, 3) }
            });

        var bodyItemToAdd = ItemTestData.CreateBodyEquipmentItem(1, "body");
        var weaponToAdd = ItemTestData.CreateBodyEquipmentItem(2, "weapon");

        var result1 = sut.CanAddItem(bodyItemToAdd.Metadata);
        var result2 = sut.CanAddItem(weaponToAdd.Metadata);

        Assert.False(result1.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result1.Error);

        Assert.False(result2.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result2.Error);
    }

    [InlineData("head")]
    [InlineData("body")]
    [InlineData("weapon")]
    [InlineData("shield")]
    [InlineData("necklace")]
    [InlineData("ring")]
    [InlineData("backpack")]
    [InlineData("feet")]
    [InlineData("ammo")]
    [Theory]
    public void CanAddItem_When_Slot_Is_Empty_And_Adding_Cumulative_Item_Returns_Success(string bodyPosition)
    {
        var item = ItemTestData.CreateCumulativeItem(1, 100, slot: bodyPosition);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, (IItem Item, ushort Id)>());

        var result = sut.CanAddItem(item.Metadata);

        Assert.True(result.Succeeded);
        Assert.Equal((uint)100, result.Value);
    }

    [InlineData(40, 60)]
    [InlineData(70, 30)]
    [InlineData(99, 1)]
    [InlineData(1, 99)]
    [InlineData(5, 95)]
    [InlineData(50, 50)]
    [Theory]
    public void CanAddItem_When_Slot_Has_Same_Cumulative_Item_And_Adding_Cumulative_Item_Returns_Success(byte amount,
        uint expected)
    {
        var item = ItemTestData.CreateAmmo(1, 100);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Ammo, (ItemTestData.CreateAmmo(1, amount), 1) }
            });

        var result = sut.CanAddItem(item.Metadata);

        Assert.True(result.Succeeded);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void CanAddItem_When_Slot_Has_Different_Cumulative_Item_And_Adding_Cumulative_Item_Returns_Not_Enough_Room()
    {
        var item = ItemTestData.CreateAmmo(2, 100);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, (IItem Item, ushort Id)>
            {
                { Slot.Ammo, (ItemTestData.CreateAmmo(1, 50), 1) }
            });

        var result = sut.CanAddItem(item.Metadata);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result.Error);
    }

    [Fact]
    public void CanAddItem_When_PlayerHasNoRequiredLevel_ReturnsFalse()
    {
        //arrange
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var sut = PlayerTestDataBuilder.Build(skills: skills);

        var bodyItemToAdd = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.MinimumLevel, 1000)
        });

        //act
        var actual = sut.Inventory.AddItem(bodyItemToAdd, (byte)Slot.Body);

        //assert
        actual.Error.Should().Be(InvalidOperation.CannotDress);
    }

    [Fact]
    public void DressingEquipments_PlayerHasAllEquipments_ReturnAllExceptBag()
    {
        //arrange
        var inventory = PlayerTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(inventoryMap: inventory, capacity: 500_000);
        var expected = inventory.Where(x => x.Key != Slot.Backpack).Select(x => x.Value.Item1);

        //assert
        player.Inventory.DressingEquipments.Should().HaveSameCount(expected);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Head].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Necklace].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Ring].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Body].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Left].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Ammo].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Legs].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Ring].Item1);
        player.Inventory.DressingEquipments.Should().ContainSingle(x => x == inventory[Slot.Feet].Item1);
    }

    [Fact]
    public void Player_cannot_add_a_non_pickupable_item_to_inventory()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();

        var item = ItemTestData.CreateTopItem(1, 1);

        //act
        var result = inventory.AddItem(item);

        //assert
        result.Error.Should().Be(InvalidOperation.NotPossible);
    }

    [Fact]
    public void Player_cannot_remove_an_item_with_no_amount_from_inventory()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();

        var item = ItemTestData.CreateAmmo(1, 10);

        inventory.AddItem(item, Slot.Ammo);

        //act
        var result = inventory.RemoveItem(Slot.Ammo, 0);

        //assert
        result.Error.Should().Be(InvalidOperation.Impossible);
    }

    [Fact]
    public void Player_cannot_remove_an_item_that_is_not_on_inventory()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();

        var item = ItemTestData.CreateAmmo(1, 10);

        inventory.AddItem(item, Slot.Ammo);

        //act
        var result = inventory.RemoveItem(item, 1, (byte)Slot.Body, out _);

        //assert
        result.Error.Should().Be(InvalidOperation.Impossible);
    }

    [Fact]
    public void Cumulative_item_is_removed_from_inventory_when_reach_0_amount()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();

        var item = (ICumulative)ItemTestData.CreateAmmo(1, 1);

        inventory.AddItem(item, Slot.Ammo);

        //act
        item.Reduce();

        //assert
        inventory[Slot.Ammo].Should().BeNull();
    }

    [Theory]
    [InlineData(Slot.Left, true)]
    [InlineData(Slot.Backpack, false)]
    public void Player_is_using_weapon_when_have_a_weapon_on_left_slot(Slot slot, bool expectedResult)
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var backpack = ItemTestData.CreateBackpack();

        inventory.AddItem(backpack, Slot.Backpack);

        var item = ItemTestData.CreateWeaponItem(1);

        inventory.AddItem(item, slot);

        //assert
        inventory.IsUsingWeapon.Should().Be(expectedResult);
    }

    [Fact]
    public void Inventory_map_returns_all_items()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(capacity: 5000);
        var inventory = InventoryTestDataBuilder.Build(player);

        var backpack = ItemTestData.CreateBackpack();
        var helmet = ItemTestData.CreateBodyEquipmentItem(2, "head");
        var armor = ItemTestData.CreateBodyEquipmentItem(3, "body");
        var shield = ItemTestData.CreateBodyEquipmentItem(4, "shield");
        var weapon = ItemTestData.CreateWeaponItem(5);
        var boots = ItemTestData.CreateBodyEquipmentItem(6, "feet");
        var legs = ItemTestData.CreateBodyEquipmentItem(7, "legs");
        var ammo = ItemTestData.CreateAmmo(8, 20);
        var ring = ItemTestData.CreateBodyEquipmentItem(9, "ring");

        var moreAmmo = ItemTestData.CreateAmmo(8, 40);
        var anotherLegs = ItemTestData.CreateBodyEquipmentItem(7, "legs");
        var anotherArmor = ItemTestData.CreateBodyEquipmentItem(10, "armor");

        var anotherBackpack = ItemTestData.CreateBackpack();
        var anotherShield = ItemTestData.CreateBodyEquipmentItem(4, "shield");

        inventory.AddItem(backpack, Slot.Backpack);
        inventory.AddItem(helmet, Slot.Head);
        inventory.AddItem(armor, Slot.Body);
        inventory.AddItem(shield, Slot.Right);
        inventory.AddItem(weapon, Slot.Left);
        inventory.AddItem(boots, Slot.Feet);
        inventory.AddItem(legs, Slot.Legs);
        inventory.AddItem(ammo, Slot.Ammo);
        inventory.AddItem(ring, Slot.Ring);

        inventory.AddItem(moreAmmo, Slot.Backpack);
        inventory.AddItem(anotherLegs, Slot.Backpack);
        inventory.AddItem(anotherArmor, Slot.Backpack);
        inventory.AddItem(anotherBackpack, Slot.Backpack);
        anotherBackpack.AddItem(anotherShield);

        //assert
        inventory.Map[1].Should().Be(2);
        inventory.Map[2].Should().Be(1);
        inventory.Map[3].Should().Be(1);
        inventory.Map[4].Should().Be(2);
        inventory.Map[5].Should().Be(1);
        inventory.Map[6].Should().Be(1);
        inventory.Map[7].Should().Be(2);
        inventory.Map[8].Should().Be(60);
        inventory.Map[9].Should().Be(1);
        inventory.Map[10].Should().Be(1);
    }

    #region Mock data

    public static IEnumerable<object[]> SlotItemsData =>
        new List<object[]>
        {
            new object[] { Slot.Backpack, ItemTestData.CreateBackpack() },
            new object[] { Slot.Ammo, ItemTestData.CreateAmmo(100, 10) },
            new object[] { Slot.Head, ItemTestData.CreateBodyEquipmentItem(100, "head") },
            new object[] { Slot.Left, ItemTestData.CreateWeaponItem(100, "axe") },
            new object[] { Slot.Body, ItemTestData.CreateBodyEquipmentItem(100, "body") },
            new object[] { Slot.Feet, ItemTestData.CreateBodyEquipmentItem(100, "feet") },
            new object[] { Slot.Right, ItemTestData.CreateBodyEquipmentItem(100, "", "shield") },
            new object[] { Slot.Ring, ItemTestData.CreateBodyEquipmentItem(100, "ring") },
            new object[] { Slot.Left, ItemTestData.CreateWeaponItem(100, weaponType: "sword", twoHanded: true) },
            new object[] { Slot.Necklace, ItemTestData.CreateBodyEquipmentItem(100, "necklace") }
        };

    public static IEnumerable<object[]> BackpackSlotAddItemsData =>
        new List<object[]>
        {
            new object[] { ItemTestData.CreateAmmo(100, 10) },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "head") },
            new object[] { ItemTestData.CreateWeaponItem(100, "axe") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "body") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "feet") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "", "shield") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "ring") },
            new object[] { ItemTestData.CreateWeaponItem(100, weaponType: "sword", twoHanded: true) },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "necklace") },
            new object[] { ItemTestData.CreateCumulativeItem(100, 87) }
        };

    public static IEnumerable<object[]> SlotSwapItemsData =>
        new List<object[]>
        {
            new object[] { Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(102, 10) },
            new object[]
            {
                Slot.Head, ItemTestData.CreateBodyEquipmentItem(100, "head"),
                ItemTestData.CreateBodyEquipmentItem(102, "head")
            },
            new object[]
                { Slot.Left, ItemTestData.CreateWeaponItem(100, "axe"), ItemTestData.CreateWeaponItem(102, "axe") },
            new object[]
            {
                Slot.Body, ItemTestData.CreateBodyEquipmentItem(100, "body"),
                ItemTestData.CreateBodyEquipmentItem(102, "body")
            },
            new object[]
            {
                Slot.Feet, ItemTestData.CreateBodyEquipmentItem(100, "feet"),
                ItemTestData.CreateBodyEquipmentItem(102, "feet")
            },
            new object[]
            {
                Slot.Right, ItemTestData.CreateBodyEquipmentItem(100, "", "shield"),
                ItemTestData.CreateBodyEquipmentItem(102, "", "shield")
            },
            new object[]
            {
                Slot.Ring, ItemTestData.CreateBodyEquipmentItem(100, "ring"),
                ItemTestData.CreateBodyEquipmentItem(102, "ring")
            },
            new object[]
            {
                Slot.Left, ItemTestData.CreateWeaponItem(100, weaponType: "sword", twoHanded: true),
                ItemTestData.CreateWeaponItem(102, weaponType: "sword", twoHanded: true)
            },
            new object[]
            {
                Slot.Necklace, ItemTestData.CreateBodyEquipmentItem(100, "necklace"),
                ItemTestData.CreateBodyEquipmentItem(102, "necklace")
            }
        };

    public static IEnumerable<object[]> SlotJoinItemsData =>
        new List<object[]>
        {
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(100, 10),
                ItemTestData.CreateAmmo(100, 20)
            },
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(100, 90),
                ItemTestData.CreateAmmo(100, 100)
            },
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 50), ItemTestData.CreateAmmo(100, 90),
                ItemTestData.CreateAmmo(100, 100)
            },
            new object[]
            {
                Slot.Left, ItemTestData.CreateThrowableDistanceItem(100),
                ItemTestData.CreateThrowableDistanceItem(100, 5), ItemTestData.CreateThrowableDistanceItem(100, 6)
            }
        };

    public static IEnumerable<object[]> WrongSlotItemsData => GenerateWrongSlotItemsData();

    private static List<object[]> GenerateWrongSlotItemsData()
    {
        var result = new List<object[]>();
        foreach (var slot in new List<Slot>
                 {
                     Slot.Head, Slot.Ammo, Slot.Backpack, Slot.Body, Slot.Feet, Slot.Left, Slot.Right, Slot.Ring,
                     Slot.TwoHanded,
                     Slot.Legs, Slot.Necklace
                 })
        {
            if (slot != Slot.Body)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "body") });

            if (slot != Slot.Ammo)
                result.Add(new object[] { slot, ItemTestData.CreateAmmo(100, 10) });

            if (slot != Slot.Legs)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "legs") });

            if (slot != Slot.Feet)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "feet") });

            if (slot != Slot.Right)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "", "shield") });

            if (slot != Slot.Left)
                result.Add(new object[] { slot, ItemTestData.CreateWeaponItem(100, "axe") });

            if (slot != Slot.Ring)
                result.Add(new object[] { slot, ItemTestData.CreateDefenseEquipmentItem(100) });

            if (slot != Slot.Necklace)
                result.Add(new object[] { slot, ItemTestData.CreateDefenseEquipmentItem(100) });

            if (slot != Slot.Backpack)
                result.Add(new object[] { slot, ItemTestData.CreateBackpack() });

            if (slot != Slot.Head)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "head") });
        }

        return result;
    }

    #endregion
}