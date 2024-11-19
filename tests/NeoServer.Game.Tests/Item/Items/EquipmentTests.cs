using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Item.Items.Attributes;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.Tests.Server;
using Xunit;

namespace NeoServer.Game.Tests.Item.Items;

public class EquipmentTests
{
    [Fact]
    public void DressedIn_Null_DoNotThrow()
    {
        var sut = ItemTestData.CreateDefenseEquipmentItem(1);
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.SkillAxe, 5);
        sut.DressedIn(null);
    }

    [Fact]
    public void DressedIn_Player_AddSkillBonus()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: PlayerTestDataBuilder.GenerateSkills(10));
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes:
        [
            (ItemAttribute.SkillAxe, 5),
            (ItemAttribute.Duration, 100)
        ]);
        //act
        sut.DressedIn(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);
    }

    [Fact]
    public void UndressFrom_Null_DoNotThrow()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes:
        [
            (ItemAttribute.SkillAxe, 5),
            (ItemAttribute.Duration, 100)
        ]);
        //act
        sut.UndressFrom(null);
    }

    [Fact]
    public void UndressFrom_Player_RemoveSkillBonus()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: PlayerTestDataBuilder.GenerateSkills(10));
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes:
        [
            (ItemAttribute.SkillAxe, 5),
            (ItemAttribute.Duration, 100)
        ]);

        //act
        sut.DressedIn(player);
        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);

        //act
        sut.UndressFrom(player);
        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(0);
    }

    [Fact]
    public void NoCharges_10Charges_ReturnsFalse()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10);
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.AbsorbPercentEnergy, 10);

        //assert
        sut.NoCharges.Should().BeFalse();
    }

    [Fact]
    public void NoCharges_0Charges_ReturnsTrue()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1);
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.AbsorbPercentEnergy, 10);

        //act
        sut.DecreaseCharges();

        //assert
        sut.NoCharges.Should().BeTrue();
    }

    [Fact]
    public void NoCharges_NonChargeable_ReturnsFalse()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 0);
        sut.Metadata.Attributes.SetAttribute(ItemAttribute.AbsorbPercentEnergy, 10);

        //assert
        sut.NoCharges.Should().BeFalse();
    }

    [Fact]
    public void TransformOnEquip_NoItemToTransformTo_DoNotTransform()
    {
        //arrange

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100)
            ]);

        var metadata = sut.Metadata;

        //act
        sut.TransformOnEquip();

        //assert
        metadata.Should().BeEquivalentTo(sut.Metadata);
    }

    [Fact]
    public void TransformOnEquip_HasItemToTransformTo_Transform()
    {
        //arrange

        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100),
                (ItemAttribute.TransformEquipTo, 2)
            ], itemTypeFinder: itemTypeStore.Get);

        var metadata = sut.Metadata;

        //act
        sut.TransformOnEquip();

        //assert
        metadata.Should().NotBeEquivalentTo(sut.Metadata);
        sut.Metadata.ClientId.Should().Be(2);
    }

    [Fact]
    public void TransformOnDequip_NoItemToTransformTo_DoNotTransform()
    {
        var itemTypeStore = ItemTestData.GetItemTypeStore();

        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100)
            ], itemTypeFinder: itemTypeStore.Get);

        var metadata = sut.Metadata;

        //act
        sut.TransformOnDequip();

        //assert
        metadata.Should().BeEquivalentTo(sut.Metadata);
    }

    [Fact]
    public void TransformOnDequip_HasItemToTransformTo_Transform()
    {
        //arrange
        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100),
                (ItemAttribute.TransformDequipTo, 2)
            ], itemTypeFinder: itemTypeStore.Get);

        var metadata = sut.Metadata;

        //act
        sut.TransformOnDequip();

        //assert
        metadata.Should().NotBeEquivalentTo(sut.Metadata);
        sut.Metadata.ClientId.Should().Be(2);
    }

    [Fact]
    public void DressedIn_HasItemToTransformTo_Transform()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100),
                (ItemAttribute.TransformEquipTo, 2)
            ], itemTypeFinder: itemTypeStore.Get);

        IItemType itemBefore = null;
        IItemType itemNow = null;
        sut.OnTransformed += (before, now) =>
        {
            itemBefore = before;
            itemNow = now;
        };

        var metadata = sut.Metadata;

        //act
        sut.DressedIn(player);

        //assert
        metadata.Should().NotBeEquivalentTo(sut.Metadata);
        sut.Metadata.ClientId.Should().Be(2);
        itemBefore.Should().BeEquivalentTo(metadata);
        itemNow.Should().BeEquivalentTo(sut.Metadata);
    }

    [Fact]
    public void UndressFrom_HasItemToTransformTo_Transform()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2,
            attributes:
            [
                (ItemAttribute.TransformDequipTo, 3)
            ]);

        var transformOnDequipItem = ItemTestData.CreateDefenseEquipmentItem(3);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata, transformOnDequipItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1,
            attributes:
            [
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100),
                (ItemAttribute.TransformEquipTo, 2)
            ], itemTypeFinder: itemTypeStore.Get);

        IItemType itemBefore = null;
        IItemType itemNow = null;
        sut.OnTransformed += (before, now) =>
        {
            itemBefore = before;
            itemNow = now;
        };

        var metadata = sut.Metadata;


        //assert
        sut.Metadata.ClientId.Should().Be(1);

        //act
        sut.DressedIn(player);

        //assert
        sut.Metadata.ClientId.Should().Be(2);

        var beforeUndress = sut.Metadata;

        sut.UndressFrom(player);

        //assert
        metadata.Should().NotBeEquivalentTo(sut.Metadata);
        sut.Metadata.ClientId.Should().Be(3);
        itemBefore.Should().BeEquivalentTo(beforeUndress);
        itemNow.Should().BeEquivalentTo(sut.Metadata);
    }

    [Fact]
    [ThreadBlocking]
    public void OnDecayed_NoItemToDecayTo_UndressFromPlayer()
    {
        //arrange
        var decayableItemManager = ItemDecayServiceTestBuilder.BuildTracker();

        EventSubscriptionReset.Clear<Decayable>(nameof(Decayable.OnStarted));
        Decayable.OnStarted += decayableItemManager.Track;

        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.AbsorbPercentEnergy, 100),
            (ItemAttribute.ExpireTarget, 0),
            (ItemAttribute.Duration, 1)
        ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore();
        ItemTestData.AddItemTypeStore(itemTypeStore, sut.Metadata);

        var slotRemoved = Slot.None;
        IItem itemRemoved = null;
        player.Inventory.OnItemRemovedFromSlot += (_, item, slot, _) =>
        {
            slotRemoved = slot;
            itemRemoved = item;
        };

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        Thread.Sleep(1500);
        var expiredItems = decayableItemManager.GetExpiredItems();
        ItemDecayServiceTestBuilder.BuildProcessor(null, itemTypeStore).Decay(expiredItems);
        //assert

        player.Inventory[Slot.Ring].Should().BeNull();
        slotRemoved.Should().Be(Slot.Ring);
        itemRemoved.Should().Be(sut);
    }

    [Fact]
    public void TransformOnEquip_HasDuration_SetsDuration()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2, "ring", 1,
        [
            (ItemAttribute.AbsorbPercentEnergy, 100),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.Duration, 1800)
        ]);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.AbsorbPercentEnergy, 100),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.TransformEquipTo, 2)
        ], itemTypeStore.Get);

        //assert
        sut.Decay?.Duration.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        //assert
        sut.Decay?.Duration.Should().Be(1800);
    }

    [Fact]
    public void StartDecay_NoStopDecayingAttr_Starts()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 1000),
            (ItemAttribute.ShowDuration, false)
        ]);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.ElapsedSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Item_without_decay_duration_does_not_start_decaying()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.ShowDuration, false),
            (ItemAttribute.StopDecaying, 1)
        ]);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);
    }

    [Fact]
    public void StartDecay_HasStopDecayingFalse_Start()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 1000),
            (ItemAttribute.ShowDuration, false),
            (ItemAttribute.StopDecaying, 0)
        ]);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.ElapsedSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PauseDecay_HasNoStopDecayingAttr_DoNotPause()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, false)
        ]);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, false),
            (ItemAttribute.TransformEquipTo, 2)
        ], itemTypeStore.Get);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);
        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(1100);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(2);
    }

    [Fact]
    public void PauseDecay_HasStopDecayingTrue_Pauses()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var dequipTo = ItemTestData.CreateDefenseEquipmentItem(3,
            attributes:
            [
                (ItemAttribute.StopDecaying, 1)
            ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore(dequipTo.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.TransformDequipTo, 3)
        ], itemTypeStore.Get);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1000);
        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(2000);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(1);
    }

    [Fact]
    public void PauseDecay_HasStopDecayingFalse_DoNotPause()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var transformToItem = ItemTestData.CreateDefenseEquipmentItem(2, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, false),
            (ItemAttribute.StopDecaying, 0)
        ]);

        var transformToItemDequip = ItemTestData.CreateDefenseEquipmentItem(3, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.StopDecaying, 0)
        ]);

        var itemTypeStore = ItemTestData.GetItemTypeStore(transformToItemDequip.Metadata, transformToItem.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 100),
            (ItemAttribute.ShowDuration, false),
            (ItemAttribute.TransformEquipTo, 2),
            (ItemAttribute.TransformDequipTo, 3)
        ], itemTypeStore.Get);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);

        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(2000);

        //assert
        sut.Decay?.ElapsedSeconds.Should().Be(3);
    }

    [Fact]
    public void Decayed_HasExpirationTarget_ChangeItem()
    {
        //arrange
        var itemDecayTracker = ItemDecayServiceTestBuilder.BuildTracker();

        EventSubscriptionReset.Clear<Decayable>(nameof(Decayable.OnStarted));
        Decayable.OnStarted += itemDecayTracker.Track;

        var player = PlayerTestDataBuilder.Build();

        var decaysTo = ItemTestData.CreateDefenseEquipmentItem(3, "ring",
            attributes:
            [
            ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore(decaysTo.Metadata);

        var itemDecayProcessor = ItemDecayServiceTestBuilder.BuildProcessor(null, itemTypeStore);

        //act
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 1),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.ExpireTarget, 3)
        ], itemTypeStore.Get);

        player.Inventory.AddItem(sut);

        Thread.Sleep(1200);

        var expiredItems = itemDecayTracker.GetExpiredItems();
        itemDecayProcessor.Decay(expiredItems);

        //assert
        player.Inventory[Slot.Ring].Metadata.Should().Be(decaysTo.Metadata);
    }

    [Fact]
    public void TransformOnEquip_OldItemHasNoDecayableButNewHas_CreateDecayableInstance()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformOnEquip = ItemTestData.CreateDefenseEquipmentItem(3, "ring",
            attributes:
            [
                (ItemAttribute.Duration, 100),
                (ItemAttribute.ShowDuration, 1),
                (ItemAttribute.ExpireTarget, 0)
            ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 3)
        ], itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        Thread.Sleep(1200);

        //assert
        player.Inventory[Slot.Ring].Metadata.Should().Be(transformOnEquip.Metadata);
        (player.Inventory[Slot.Ring] as IEquipment).Decay?.Duration.Should().Be(100);
    }

    [Fact]
    [ThreadBlocking]
    public void Item_disappears_when_decay_target_is_not_found()
    {
        //arrange
        var decayableItemManager = ItemDecayServiceTestBuilder.BuildTracker();

        EventSubscriptionReset.Clear<Decayable>(nameof(Decayable.OnStarted));
        Decayable.OnStarted += decayableItemManager.Track;

        var player = PlayerTestDataBuilder.Build();

        var itemTypeStore = ItemTestData.GetItemTypeStore();

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.Duration, 1),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.ExpireTarget, 5)
        ], itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);

        var expiredItems = decayableItemManager.GetExpiredItems();
        ItemDecayServiceTestBuilder.BuildProcessor(null, itemTypeStore).Decay(expiredItems);

        //assert
        player.Inventory[Slot.Ring].Should().BeNull();
    }


    [Fact(Skip = "Need to be fixed")] //TODO
    [ThreadBlocking]
    public void Item_that_decay_to_different_3_item_decays()
    {
        //arrange
        var itemDecayTracker = ItemDecayServiceTestBuilder.BuildTracker();

        EventSubscriptionReset.Clear<Decayable>(nameof(Decayable.OnStarted));
        Decayable.OnStarted += itemDecayTracker.Track;

        var player = PlayerTestDataBuilder.Build();
        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

        var itemTypeStore = ItemTestData.GetItemTypeStore();

        var item3Equipped = ItemTestData.CreateDefenseEquipmentItem(600, "ring",
            attributes:
            [
                (ItemAttribute.Duration, 2),
                (ItemAttribute.ShowDuration, 1),
                (ItemAttribute.ExpireTarget, 0),
                (ItemAttribute.TransformDequipTo, 500)
            ]);

        var item3 = ItemTestData.CreateDefenseEquipmentItem(500, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 600),
            (ItemAttribute.StopDecaying, 1),
            (ItemAttribute.ShowDuration, 1)
        ], itemTypeStore.Get);
        var item2Equipped = ItemTestData.CreateDefenseEquipmentItem(400, "ring",
            attributes:
            [
                (ItemAttribute.Duration, 2),
                (ItemAttribute.ShowDuration, 1),
                (ItemAttribute.ExpireTarget, 500),
                (ItemAttribute.TransformDequipTo, 300)
            ]);

        var item2 = ItemTestData.CreateDefenseEquipmentItem(300, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 400),
            (ItemAttribute.StopDecaying, 1),
            (ItemAttribute.ShowDuration, 1)
        ], itemTypeStore.Get);

        var item1Equipped = ItemTestData.CreateDefenseEquipmentItem(200, "ring",
            attributes:
            [
                (ItemAttribute.Duration, 2),
                (ItemAttribute.ShowDuration, 1),
                (ItemAttribute.ExpireTarget, 300),
                (ItemAttribute.TransformDequipTo, 100)
            ]);

        var item1 = ItemTestData.CreateDefenseEquipmentItem(100, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 200),
            (ItemAttribute.StopDecaying, 1),
            (ItemAttribute.ShowDuration, 1)
        ], itemTypeStore.Get);

        ItemTestData.AddItemTypeStore(itemTypeStore, item1.Metadata, item1Equipped.Metadata, item2.Metadata,
            item2Equipped.Metadata, item3.Metadata, item3Equipped.Metadata);

        var decayItemProcessor = ItemDecayServiceTestBuilder.BuildProcessor(map, itemTypeStore, itemDecayTracker);

        //assert first item
        item1.Decay?.Duration.Should().Be(0);

        //act
        player.Inventory.AddItem(item1, (byte)Slot.Ring);

        //assert first item equipped
        item1Equipped.Decay?.Duration.Should().Be(2);

        IEquipment GetSlotItem()
        {
            return player.Inventory.TryGetItem<IEquipment>(Slot.Ring);
        }

        //act
        Thread.Sleep(2050);
        var expiredItems = itemDecayTracker.GetExpiredItems();
        decayItemProcessor.Decay(expiredItems);

        player.Inventory[Slot.Ring].Metadata.TypeId.Should().Be(400);

        //assert second item equipped
        GetSlotItem().Decay?.Duration.Should().Be(2);

        //act
        Thread.Sleep(2050);

        expiredItems = itemDecayTracker.GetExpiredItems();
        decayItemProcessor.Decay(expiredItems);

        player.Inventory[Slot.Ring].Metadata.TypeId.Should().Be(600);

        //assert third item equipped
        GetSlotItem().Decay?.Duration.Should().Be(2);

        //act
        Thread.Sleep(2050);

        expiredItems = itemDecayTracker.GetExpiredItems();
        decayItemProcessor.Decay(expiredItems);

        //assert player
        player.Inventory[Slot.Ring].Should().BeNull();
    }


    [Fact]
    public void TransformOnEquip_OldItemHasNoSkillBonusButNewHas_CreateSkillBonusInstance()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformOnEquip = ItemTestData.CreateDefenseEquipmentItem(3, "ring",
            attributes:
            [
                (ItemAttribute.SkillAxe, 5)
            ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 3)
        ], itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        //assert
        player.Inventory[Slot.Ring].Metadata.Should().Be(transformOnEquip.Metadata);
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);
    }

    [Fact]
    public void TransformOnEquip_OldItemHasNoProtectionButNewHas_CreateProtectionInstance()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var combatDamage = new CombatDamage(100, DamageType.Death);

        var transformOnEquip = ItemTestData.CreateDefenseEquipmentItem(3, "ring",
            attributes:
            [
                (ItemAttribute.AbsorbPercentDeath, 5)
            ]);
        var itemTypeStore = ItemTestData.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 1,
        [
            (ItemAttribute.TransformEquipTo, 3)
        ], itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        //assert

        player.Inventory[Slot.Ring].Metadata.Should().Be(transformOnEquip.Metadata);
        sut.Protect(ref combatDamage);
        combatDamage.Damage.Should().Be(95);
    }

    [Fact]
    public void Player_swipes_item_undress_it()
    {
        //arrange 
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "body");
        var backpack = ItemTestData.CreateBackpack();

        using var monitor = sut.Monitor();

        var player = PlayerTestDataBuilder.Build(inventoryMap: new Dictionary<Slot, (IItem Item, ushort Id)>
        {
            [Slot.Backpack] = new(backpack, 3),
            [Slot.Body] = new(sut, 1)
        });

        var item = ItemTestData.CreateDefenseEquipmentItem(2, "body");
        backpack.AddItem(item);

        //act
        player.MoveItem(item, backpack, player.Inventory, 1,
            0, (byte)Slot.Body);

        //assert
        monitor.Should().Raise(nameof(sut.OnUndressed));
    }

    #region InspectionText

    [Fact]
    public void InspectionText_HasCharges_ShowChargesCount()
    {
        //arrange
        var sut = ItemTestData.CreateDefenseEquipmentItem(1, "ring", 2,
        [
            (ItemAttribute.ShowCharges, true)
        ]);

        //assert
        sut.InspectionText.Should().Be(" that has 2 charges left");
        sut.DecreaseCharges();
        sut.InspectionText.Should().Be(" that has 1 charge left");
        sut.DecreaseCharges();
        sut.InspectionText.Should().Be(" that has no charges left");
    }

    [Fact]
    public void InspectionText_HasAttribute_ReturnText()
    {
        //arrange
        var sut = ItemTestData.CreateWeaponItem(1, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20)
        ]);

        //assert
        sut.InspectionText.Should().Be("(Atk: 50, Def: 20)");
    }

    [Fact]
    public void InspectionText_HasAttributesAndSkillBonus_ReturnText()
    {
        //arrange
        var sut = ItemTestData.CreateWeaponItem(1, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20),
            (ItemAttribute.SkillAxe, 30),
            (ItemAttribute.SkillClub, 10)
        ]);

        //assert
        sut.InspectionText.Should().Be("(Atk: 50, Def: 20, axe fighting +30, club fighting +10)");
    }

    [Fact]
    public void InspectionText_HasAttributesAndSkillBonusAndProtection_ReturnText()
    {
        //arrange
        var sut = ItemTestData.CreateWeaponItem(1, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20),
            (ItemAttribute.SkillAxe, 30),
            (ItemAttribute.SkillClub, 10),
            (ItemAttribute.AbsorbPercentDeath, 60),
            (ItemAttribute.AbsorbPercentEnergy, 70)
        ]);

        //assert
        sut.InspectionText.Should()
            .Be("(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%)");
    }

    [Fact]
    public void InspectionText_AllAttributesAndDecay_ReturnText()
    {
        //arrange
        var sut = ItemTestData.CreateWeaponItem(1, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20),
            (ItemAttribute.SkillAxe, 30),
            (ItemAttribute.SkillClub, 10),
            (ItemAttribute.AbsorbPercentDeath, 60),
            (ItemAttribute.AbsorbPercentEnergy, 70),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.StopDecaying, 0)
        ]);

        //assert
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that is brand-new");

        sut = ItemTestData.CreateWeaponItem(1, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20),
            (ItemAttribute.SkillAxe, 30),
            (ItemAttribute.SkillClub, 10),
            (ItemAttribute.AbsorbPercentDeath, 60),
            (ItemAttribute.AbsorbPercentEnergy, 70),
            (ItemAttribute.Duration, 50),
            (ItemAttribute.ShowDuration, 1),
            (ItemAttribute.StopDecaying, 0)
        ]);

        (sut as IEquipment).StartDecay();
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that will expire in 0 minute and 50 seconds");
    }

    [Fact]
    public void InspectionText_AllAttributesAndCharges_ReturnText()
    {
        //arrange
        var sut = ItemTestData.CreateWeaponItem(1, charges: 10, attributes:
        [
            (ItemAttribute.Attack, 50),
            (ItemAttribute.Defense, 20),
            (ItemAttribute.SkillAxe, 30),
            (ItemAttribute.SkillClub, 10),
            (ItemAttribute.AbsorbPercentDeath, 60),
            (ItemAttribute.AbsorbPercentEnergy, 70),
            (ItemAttribute.ShowCharges, 1)
        ]);

        //assert
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that has 10 charges left");
    }

    #endregion
}