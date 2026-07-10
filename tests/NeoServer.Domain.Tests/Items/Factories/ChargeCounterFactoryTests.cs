using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Items.Attributes;

namespace NeoServer.Domain.Tests.Items.Factories;

public class ChargeCounterFactoryTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void ChargeableFactory_creates_chargeable_with_metadata_charges_when_no_override()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)200);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowCharges, (ushort)1);
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(200);
        chargeable.ShowAmount.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ChargeableFactory_creates_chargeable_with_override_charges_when_provided()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)200);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowCharges, (ushort)1);
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType, overrideCharges: 150);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(150);
        chargeable.ShowAmount.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ChargeableFactory_returns_null_when_no_charges_attribute_and_no_override()
    {
        // Arrange
        var itemType = new ItemType();
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType);

        // Assert
        chargeable.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ChargeableFactory_creates_chargeable_with_override_when_item_type_has_no_charges_attribute()
    {
        // Arrange
        var itemType = new ItemType();
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType, overrideCharges: 99);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(99);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ChargeableFactory_creates_chargeable_with_hidden_charges_when_showcharges_is_zero()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)50);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.ShowCharges, (ushort)0);
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(50);
        chargeable.ShowAmount.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void ChargeableFactory_creates_chargeable_with_showcharges_default_when_showcharges_attribute_missing()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)50);
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(50);
        chargeable.ShowAmount.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void ChargeableFactory_creates_chargeable_with_zero_override_charges()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)200);
        var sut = new ChargeCounterFactory();

        // Act
        var chargeable = sut.Create(itemType, overrideCharges: 0);

        // Assert
        chargeable.Should().NotBeNull();
        chargeable.Amount.Should().Be(0);
        chargeable.IsEmpty.Should().BeTrue();
    }
}
