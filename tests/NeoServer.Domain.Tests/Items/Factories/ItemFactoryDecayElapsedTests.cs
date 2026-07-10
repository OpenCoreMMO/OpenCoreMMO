using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Tests.Items.Factories;

public class ItemFactoryDecayElapsedTests
{
    private static IItemType CreateDecayableItemType(ushort id, uint duration, ushort decaysTo = 0)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName("decayable item");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "necklace");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Duration, duration);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowDuration, (ushort)1);
        if (decaysTo > 0)
            itemType.Attributes.SetAttribute(ItemTypeAttribute.ExpireTarget, decaysTo);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)10);
        itemType.SetGroupIfNone();
        return itemType;
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemFactory_sets_decay_elapsed_when_override_provided()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)120 }
        };

        // Act
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(120);
        createdItem.Decay.Remaining.Should().Be(180);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemFactory_sets_zero_decay_elapsed_when_zero_override()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)0 }
        };

        // Act
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(0);
        createdItem.Decay.Remaining.Should().Be(300);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemFactory_leaves_default_elapsed_when_no_override()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        // Act - no DecayElapsed override
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), null, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemFactory_does_not_throw_when_decay_elapsed_provided_for_non_decayable_item()
    {
        // Arrange - item without Duration attribute (not decayable)
        var itemType = new ItemType();
        itemType.SetId(100);
        itemType.SetClientId(100);
        itemType.SetName("regular item");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)10);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var genericItemFactory = new GenericItemFactory();
        var sut = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, genericItemFactory, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)50 }
        };

        // Act — slot/location is irrelevant for this test
        var createdItem = sut.Create(100, Location.Inventory(Slot.None), itemTypeAttributes, null, null, null);

        // Assert - should not throw and decay should be null for non-decayable item
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemType_overload_sets_decay_elapsed_when_override_provided()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)120 }
        };

        // Act
        var createdItem = sut.Create(itemType, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(120);
        createdItem.Decay.Remaining.Should().Be(180);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemType_overload_sets_zero_decay_elapsed_when_zero_override()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)0 }
        };

        // Act
        var createdItem = sut.Create(itemType, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(0);
        createdItem.Decay.Remaining.Should().Be(300);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemType_overload_leaves_default_elapsed_when_no_override()
    {
        // Arrange
        var itemType = CreateDecayableItemType(100, duration: 300);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        // Act - no DecayElapsed override
        var createdItem = sut.Create(itemType, Location.Inventory(Slot.Necklace), null, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().NotBeNull();
        createdItem.Decay.Elapsed.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemType_overload_does_not_throw_when_decay_elapsed_provided_for_non_decayable_item()
    {
        // Arrange - item without Duration attribute (not decayable)
        var itemType = new ItemType();
        itemType.SetId(100);
        itemType.SetClientId(100);
        itemType.SetName("regular item");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)10);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeCounterFactory();
        var genericItemFactory = new GenericItemFactory();
        var sut = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, genericItemFactory, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.DecayElapsed, (uint)50 }
        };

        // Act — slot/location is irrelevant for this test
        var createdItem = sut.Create(itemType, Location.Inventory(Slot.None), itemTypeAttributes, null, null, null);

        // Assert - should not throw and decay should be null for non-decayable item
        createdItem.Should().NotBeNull();
        createdItem.Decay.Should().BeNull();
    }

}
