using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items.Weapons;
using NeoServer.Domain.Tests.Server;

namespace NeoServer.Domain.Tests.Items.Factories;

public class ItemFactoryChargesTests
{
    private static IItemType CreateDefenseEquipmentType(ushort id, ushort defaultCharges, string slot = "necklace")
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName("chargeable item");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, slot);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, defaultCharges);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowCharges, (ushort)1);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)10);
        itemType.SetGroupIfNone();
        return itemType;
    }

    private static IItemType CreateMeleeWeaponType(ushort id, ushort defaultCharges)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName("chargeable weapon");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, "sword");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "weapon");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, defaultCharges);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowCharges, (ushort)1);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)40);
        itemType.SetGroupIfNone();
        return itemType;
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemFactory_creates_defense_equipment_with_override_charges()
    {
        // Arrange
        var itemType = CreateDefenseEquipmentType(100, defaultCharges: 200);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.Charges, (ushort)150 }
        };

        // Act
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Should().BeAssignableTo<IChargeable>();
        ((IChargeable)createdItem).Charges.Should().Be(150);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemFactory_creates_defense_equipment_with_default_charges_when_no_override()
    {
        // Arrange
        var itemType = CreateDefenseEquipmentType(100, defaultCharges: 200);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        // Act - no charges override in itemTypeAttributes
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), null, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Should().BeAssignableTo<IChargeable>();
        ((IChargeable)createdItem).Charges.Should().Be(200);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemFactory_creates_weapon_with_override_charges()
    {
        // Arrange
        var itemType = CreateMeleeWeaponType(100, defaultCharges: 100);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.Charges, (ushort)75 }
        };

        // Act
        var createdItem = sut.Create(100, Location.Inventory(Slot.Left), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Should().BeOfType<MeleeWeapon>();
        createdItem.Should().BeAssignableTo<IChargeable>();
        ((IChargeable)createdItem).Charges.Should().Be(75);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ItemFactory_creates_weapon_with_default_charges_when_no_override()
    {
        // Arrange
        var itemType = CreateMeleeWeaponType(100, defaultCharges: 100);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        // Act - no charges override in itemTypeAttributes
        var createdItem = sut.Create(100, Location.Inventory(Slot.Left), null, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Should().BeOfType<MeleeWeapon>();
        createdItem.Should().BeAssignableTo<IChargeable>();
        ((IChargeable)createdItem).Charges.Should().Be(100);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemFactory_creates_defense_equipment_with_zero_override_charges()
    {
        // Arrange
        var itemType = CreateDefenseEquipmentType(100, defaultCharges: 200);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.Charges, (ushort)0 }
        };

        // Act
        var createdItem = sut.Create(100, Location.Inventory(Slot.Necklace), itemTypeAttributes, null, null, null);

        // Assert
        createdItem.Should().NotBeNull();
        createdItem.Should().BeAssignableTo<IChargeable>();
        ((IChargeable)createdItem).Charges.Should().Be(0);
        ((IChargeable)createdItem).NoCharges.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ItemFactory_does_not_throw_when_charges_override_present_for_non_matching_item_type()
    {
        // Arrange - a regular item that doesn't match any specific factory
        var itemType = new ItemType();
        itemType.SetId(100);
        itemType.SetClientId(100);
        itemType.SetName("regular item");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, (ushort)10);
        var itemTypeStore = new ItemTypeStore();
        itemTypeStore.AddOrUpdate(itemType.ServerId, itemType);

        var chargeableFactory = new ChargeableFactory();
        var sut = new ItemFactory(
            null,
            null,
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);

        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible>
        {
            { ItemTypeAttribute.Charges, (ushort)50 }
        };

        // Act & Assert - should not throw even though the item type doesn't match any factory
        var exception = Record.Exception(() =>
            sut.Create(100, Location.Inventory(Slot.Backpack), itemTypeAttributes, null, null, null));
        exception.Should().BeNull();
    }
}
