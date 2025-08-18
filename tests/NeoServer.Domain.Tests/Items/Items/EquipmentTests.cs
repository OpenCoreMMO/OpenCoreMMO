using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;

namespace NeoServer.Domain.Tests.Items.Items;

public class EquipmentTests : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    [ThreadBlocking]
    public Task DisposeAsync()
    {
        // Cleanup logic after each test
        EventSubscriptionCleanUp.CleanUp<Decayable>(nameof(Decayable.OnStarted));
        return Task.CompletedTask;
    }

    [Fact]
    public void DressedIn_Null_DoNotThrow()
    {
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.SkillAxe, 5);
        sut.DressedIn(null);
    }

    [Fact]
    public void DressedIn_Player_AddSkillBonus()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: PlayerTestDataBuilder.GenerateSkills(10));
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.SkillAxe, 5),
                (ItemTypeAttribute.Duration, 100)
            });
        //act
        sut.DressedIn(player);

        //assert
        player.GetSkillBonus(SkillType.Axe).Should().Be(5);
    }

    [Fact]
    public void UndressFrom_Null_DoNotThrow()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.SkillAxe, 5),
                (ItemTypeAttribute.Duration, 100)
            });
        //act
        sut.UndressFrom(null);
    }

    [Fact]
    public void UndressFrom_Player_RemoveSkillBonus()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build(skills: PlayerTestDataBuilder.GenerateSkills(10));
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.SkillAxe, 5),
                (ItemTypeAttribute.Duration, 100)
            });

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
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 10);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.AbsorbPercentEnergy, 10);

        //assert
        sut.NoCharges.Should().BeFalse();
    }

    [Fact]
    public void NoCharges_0Charges_ReturnsTrue()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.AbsorbPercentEnergy, 10);

        //act
        sut.DecreaseCharges();

        //assert
        sut.NoCharges.Should().BeTrue();
    }

    [Fact]
    public void NoCharges_NonChargeable_ReturnsFalse()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 0);
        sut.Metadata.Attributes.SetAttribute(ItemTypeAttribute.AbsorbPercentEnergy, 10);

        //assert
        sut.NoCharges.Should().BeFalse();
    }

    [Fact]
    public void TransformOnEquip_NoItemToTransformTo_DoNotTransform()
    {
        //arrange

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100)
            });

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

        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.TransformEquipTo, 2)
            }, itemTypeFinder: itemTypeStore.Get);

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
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();

        //arrange
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100)
            }, itemTypeFinder: itemTypeStore.Get);

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
        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.TransformDequipTo, 2)
            }, itemTypeFinder: itemTypeStore.Get);

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
        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.TransformEquipTo, 2)
            }, itemTypeFinder: itemTypeStore.Get);

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

        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformDequipTo, 3)
            });

        var transformOnDequipItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(3);

        var itemTypeStore =
            ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata, transformOnDequipItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, charges: 1,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.TransformEquipTo, 2)
            }, itemTypeFinder: itemTypeStore.Get);

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
        var player = PlayerTestDataBuilder.Build();


        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.Duration, 1)
            });
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();
        ItemTestDataBuilder.AddItemTypeStore(itemTypeStore, sut.Metadata);

        var slotRemoved = Slot.None;
        IItem itemRemoved = null;
        player.Inventory.OnItemRemovedFromSlot += (_, item, slot, _) =>
        {
            slotRemoved = slot;
            itemRemoved = item;
        };

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(null, itemTypeStore);
        Decayable.OnStarted += decayableItemManager.Add;

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        Thread.Sleep(1500);
        decayableItemManager.DecayExpiredItems();
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

        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.Duration, 1800)
            });

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentEnergy, 100),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.TransformEquipTo, 2)
            }, null, itemTypeStore.Get);

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

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 1000),
                (ItemTypeAttribute.ShowDuration, false)
            });

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.Elapsed.Should().BeGreaterThan(0);
    }

    [Fact]
    public void StartDecay_HasStopDecayingTrue_DoNotStart()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 1000),
                (ItemTypeAttribute.ShowDuration, false),
                (ItemTypeAttribute.StopDecaying, 1)
            });

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.Elapsed.Should().Be(0);
    }

    [Fact]
    public void StartDecay_HasStopDecayingFalse_Start()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 1000),
                (ItemTypeAttribute.ShowDuration, false),
                (ItemTypeAttribute.StopDecaying, 0)
            });

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1500);

        //assert
        sut.Decay?.Elapsed.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PauseDecay_HasNoStopDecayingAttr_DoNotPause()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, false)
            });

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, false),
                (ItemTypeAttribute.TransformEquipTo, 2)
            }, null, itemTypeStore.Get);

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);
        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(1100);

        //assert
        sut.Decay?.Elapsed.Should().Be(3);
    }

    [Fact]
    public void PauseDecay_HasStopDecayingTrue_Pauses()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var dequipTo = ItemTestDataBuilder.CreateDefenseEquipmentItem(3,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.StopDecaying, 1)
            });
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(dequipTo.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.TransformDequipTo, 3)
            }, null, itemTypeStore.Get);

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1000);
        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(2000);

        //assert
        sut.Decay?.Elapsed.Should().Be(1);
    }

    [Fact]
    public void PauseDecay_HasStopDecayingFalse_DoNotPause()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var transformToItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(2, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, false),
                (ItemTypeAttribute.StopDecaying, 0)
            });

        var transformToItemDequip = ItemTestDataBuilder.CreateDefenseEquipmentItem(3, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.StopDecaying, 0)
            });

        var itemTypeStore =
            ItemTestDataBuilder.GetItemTypeStore(transformToItemDequip.Metadata, transformToItem.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, false),
                (ItemTypeAttribute.TransformEquipTo, 2),
                (ItemTypeAttribute.TransformDequipTo, 3)
            }, null, itemTypeStore.Get);

        //assert
        sut.Decay?.Elapsed.Should().Be(0);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);

        player.Inventory.RemoveItem(Slot.Ring, 1);
        Thread.Sleep(2000);

        //assert
        sut.Decay?.Elapsed.Should().Be(4);
    }

    [Fact]
    public void Decayed_HasExpirationTarget_ChangeItem()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var decaysTo = ItemTestDataBuilder.CreateDefenseEquipmentItem(3, "ring",
            itemTypeAttributes: Array.Empty<(ItemTypeAttribute, IConvertible)>());

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(decaysTo.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 1),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 3)
            }, null, itemTypeStore.Get);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(null, itemTypeStore);
        Decayable.OnStarted += decayableItemManager.Add;

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        Thread.Sleep(1200);
        decayableItemManager.DecayExpiredItems();

        //assert
        player.Inventory[Slot.Ring].Metadata.Should().Be(decaysTo.Metadata);
    }

    [Fact]
    public void TransformOnEquip_OldItemHasNoDecayableButNewHas_CreateDecayableInstance()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformOnEquip = ItemTestDataBuilder.CreateDefenseEquipmentItem(3, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 100),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 0)
            });
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 3)
            }, null, itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        Thread.Sleep(1200);

        //assert
        player.Inventory[Slot.Ring].Metadata.Should().Be(transformOnEquip.Metadata);
        (player.Inventory[Slot.Ring] as IEquipment).Decay?.Duration.Should().Be(100);
    }

    [Fact]
    public void Decayed_HasExpirationTargetButNoFound_OnlyRemovesItem()
    {
        //arrange

        var player = PlayerTestDataBuilder.Build();

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();
        var decayableItemManager = DecayableItemManagerTestBuilder.Build(null, itemTypeStore);
        Decayable.OnStarted += decayableItemManager.Add;

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 1),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 5)
            }, null, itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);
        Thread.Sleep(1100);
        decayableItemManager.DecayExpiredItems();

        //assert
        player.Inventory[Slot.Ring].Should().BeNull();
    }

    [Fact]
    [ThreadBlocking]
    public async Task Item_that_decay_to_different_3_item_decays()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var map = MapTestDataBuilder.Build(100, 101, 100, 101, 7, 7);

        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore();

        var item3Equipped = ItemTestDataBuilder.CreateDefenseEquipmentItem(600, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 2),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 0),
                (ItemTypeAttribute.TransformDequipTo, 500)
            });

        var item3 = ItemTestDataBuilder.CreateDefenseEquipmentItem(500, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 600),
                (ItemTypeAttribute.StopDecaying, 1),
                (ItemTypeAttribute.ShowDuration, 1)
            }, null, itemTypeStore.Get);
        var item2Equipped = ItemTestDataBuilder.CreateDefenseEquipmentItem(400, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 2),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 500),
                (ItemTypeAttribute.TransformDequipTo, 300)
            });

        var item2 = ItemTestDataBuilder.CreateDefenseEquipmentItem(300, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 400),
                (ItemTypeAttribute.StopDecaying, 1),
                (ItemTypeAttribute.ShowDuration, 1)
            }, null, itemTypeStore.Get);

        var item1Equipped = ItemTestDataBuilder.CreateDefenseEquipmentItem(200, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Duration, 2),
                (ItemTypeAttribute.ShowDuration, 1),
                (ItemTypeAttribute.ExpireTarget, 300),
                (ItemTypeAttribute.TransformDequipTo, 100)
            });

        var item1 = ItemTestDataBuilder.CreateDefenseEquipmentItem(100, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 200),
                (ItemTypeAttribute.StopDecaying, 1),
                (ItemTypeAttribute.ShowDuration, 1)
            }, null, itemTypeStore.Get);

        ItemTestDataBuilder.AddItemTypeStore(itemTypeStore, item1.Metadata, item1Equipped.Metadata, item2.Metadata,
            item2Equipped.Metadata, item3.Metadata, item3Equipped.Metadata);

        var decayableItemManager = DecayableItemManagerTestBuilder.Build(map, itemTypeStore);
        Decayable.OnStarted += decayableItemManager.Add;

        //assert first item
        item1.Decay?.Duration.Should().Be(0);
        item1.Metadata.Should().Be(item1.Metadata);

        //act
        player.Inventory.AddItem(item1, (byte)Slot.Ring);

        //assert first item equipped
        item1Equipped.Decay?.Duration.Should().Be(2);

        IEquipment GetSlotItem()
        {
            return player.Inventory.TryGetItem<IEquipment>(Slot.Ring);
        }

        //act
        await Task.Delay(2050);
        decayableItemManager.DecayExpiredItems();

        player.Inventory[Slot.Ring].Metadata.ServerId.Should().Be(400);

        //assert second item equipped
        GetSlotItem().Decay?.Duration.Should().Be(2);

        //act
        await Task.Delay(2050);
        decayableItemManager.DecayExpiredItems();
        player.Inventory[Slot.Ring].Metadata.ServerId.Should().Be(600);

        //assert third item equipped
        GetSlotItem().Decay?.Duration.Should().Be(2);

        //act
        await Task.Delay(2050);
        decayableItemManager.DecayExpiredItems();

        //assert player
        player.Inventory[Slot.Ring].Should().BeNull();
    }


    [Fact]
    public void TransformOnEquip_OldItemHasNoSkillBonusButNewHas_CreateSkillBonusInstance()
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();

        var transformOnEquip = ItemTestDataBuilder.CreateDefenseEquipmentItem(3, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.SkillAxe, 5)
            });
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 3)
            }, null, itemTypeStore.Get);

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

        var transformOnEquip = ItemTestDataBuilder.CreateDefenseEquipmentItem(3, "ring",
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.AbsorbPercentDeath, 5)
            });
        var itemTypeStore = ItemTestDataBuilder.GetItemTypeStore(transformOnEquip.Metadata);

        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 1,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.TransformEquipTo, 3)
            }, null, itemTypeStore.Get);

        //act
        player.Inventory.AddItem(sut, (byte)Slot.Ring);

        //assert

        player.Inventory[Slot.Ring].Metadata.Should().Be(transformOnEquip.Metadata);
        sut.Protect(combatDamage);
        combatDamage.Damage.Should().Be(95);
    }

    [Fact]
    public void Player_swipes_item_undress_it()
    {
        //arrange 
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "body");
        var backpack = ItemTestDataBuilder.CreateBackpack();

        using var monitor = sut.Monitor();

        var player = PlayerTestDataBuilder.Build(inventoryMap: new Dictionary<Slot, (IItem Item, ushort Id)>
        {
            [Slot.Backpack] = new(backpack, 3),
            [Slot.Body] = new(sut, 1)
        });

        var item = ItemTestDataBuilder.CreateDefenseEquipmentItem(2, "body");
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
        var sut = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, "ring", 2,
            new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.ShowCharges, true)
            });

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
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.Attack, 50),
            (ItemTypeAttribute.Defense, 20)
        });

        //assert
        sut.InspectionText.Should().Be("(Atk: 50, Def: 20)");
    }

    [Fact]
    public void InspectionText_HasAttributesAndSkillBonus_ReturnText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.Attack, 50),
            (ItemTypeAttribute.Defense, 20),
            (ItemTypeAttribute.SkillAxe, 30),
            (ItemTypeAttribute.SkillClub, 10)
        });

        //assert
        sut.InspectionText.Should().Be("(Atk: 50, Def: 20, axe fighting +30, club fighting +10)");
    }

    [Fact]
    public void InspectionText_HasAttributesAndSkillBonusAndProtection_ReturnText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.Attack, 50),
            (ItemTypeAttribute.Defense, 20),
            (ItemTypeAttribute.SkillAxe, 30),
            (ItemTypeAttribute.SkillClub, 10),
            (ItemTypeAttribute.AbsorbPercentDeath, 60),
            (ItemTypeAttribute.AbsorbPercentEnergy, 70)
        });

        //assert
        sut.InspectionText.Should()
            .Be("(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%)");
    }

    [Fact]
    public void InspectionText_AllAttributesAndDecay_ReturnText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.Attack, 50),
            (ItemTypeAttribute.Defense, 20),
            (ItemTypeAttribute.SkillAxe, 30),
            (ItemTypeAttribute.SkillClub, 10),
            (ItemTypeAttribute.AbsorbPercentDeath, 60),
            (ItemTypeAttribute.AbsorbPercentEnergy, 70),
            (ItemTypeAttribute.Duration, 50),
            (ItemTypeAttribute.ShowDuration, 1),
            (ItemTypeAttribute.StopDecaying, 0)
        });

        //assert
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that is brand-new");

        (sut as IEquipment).StartDecay();
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that will expire in 0 minute and 49 seconds");
    }

    [Fact]
    public void InspectionText_AllAttributesAndCharges_ReturnText()
    {
        //arrange
        var sut = ItemTestDataBuilder.CreateWeaponItem(1, charges: 10,
            itemTypeAttributes: new (ItemTypeAttribute, IConvertible)[]
            {
                (ItemTypeAttribute.Attack, 50),
                (ItemTypeAttribute.Defense, 20),
                (ItemTypeAttribute.SkillAxe, 30),
                (ItemTypeAttribute.SkillClub, 10),
                (ItemTypeAttribute.AbsorbPercentDeath, 60),
                (ItemTypeAttribute.AbsorbPercentEnergy, 70),
                (ItemTypeAttribute.ShowCharges, 1)
            });

        //assert
        sut.InspectionText.Should()
            .Be(
                "(Atk: 50, Def: 20, axe fighting +30, club fighting +10, protection death +60%, energy +70%) that has 10 charges left");
    }

    #endregion
}