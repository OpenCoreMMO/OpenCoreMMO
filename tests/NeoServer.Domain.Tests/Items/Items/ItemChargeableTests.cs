using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Items.Items.Weapons;
using NeoServer.Domain.Tests.Helpers;

namespace NeoServer.Domain.Tests.Items.Items;

public class ItemChargeableTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void Item_GetSubType_returns_chargeable_charges_when_item_implements_IChargeable()
    {
        // Arrange
        var defenseItem = ItemTestDataBuilder.CreateDefenseEquipmentItem(1, slot: "necklace", charges: 200);
        defenseItem.Chargeable.DecreaseCharges();
        defenseItem.Chargeable.DecreaseCharges();

        // Act
        var subType = ((IItem)defenseItem).GetSubType();

        // Assert
        subType.Should().Be(198);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Item_GetSubType_returns_chargeable_charges_when_item_is_weapon_with_charges()
    {
        // Arrange
        var meleeWeapon = (MeleeWeapon)ItemTestDataBuilder.CreateWeaponItem(100, charges: 100);
        for (var i = 0; i < 10; i++)
            meleeWeapon.Chargeable.DecreaseCharges();

        // Act
        var subType = ((IItem)meleeWeapon).GetSubType();

        // Assert
        subType.Should().Be(90);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Item_GetSubType_returns_metadata_charges_when_item_does_not_implement_IChargeable()
    {
        // Arrange
        var item = ItemTestDataBuilder.CreateRegularItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Charges, (ushort)50)
            ]);

        // Act
        var subType = ((IItem)item).GetSubType();

        // Assert
        subType.Should().Be(50);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Item_GetSubType_returns_zero_when_neither_chargeable_nor_metadata_has_charges()
    {
        // Arrange
        var item = ItemTestDataBuilder.CreateRegularItem(1);

        // Act
        var subType = ((IItem)item).GetSubType();

        // Assert
        subType.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Item_GetSubType_returns_zero_for_chargeable_item_with_null_chargeable()
    {
        // Arrange
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Charges, (ushort)75);
        var item = new BodyDefenseEquipment(itemType, new(100, 100, 7))
        {
            Chargeable = null
        };

        // Act
        var subType = ((IItem)item).GetSubType();

        // Assert
        // When Chargeable is null, the type implements IChargeable so the pattern matches,
        // but Charges delegates to Chargeable?.Charges ?? 0, returning 0.
        subType.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Chargeable_DecreaseCharges_decrements_charges_correctly()
    {
        // Arrange
        var chargeable = new Chargeable(200, true);

        // Act
        chargeable.DecreaseCharges();
        chargeable.DecreaseCharges();
        chargeable.DecreaseCharges();

        // Assert
        chargeable.Charges.Should().Be(197);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Chargeable_DecreaseCharges_does_not_go_below_zero()
    {
        // Arrange
        var chargeable = new Chargeable(0, true);

        // Act
        chargeable.DecreaseCharges();

        // Assert
        chargeable.Charges.Should().Be(0);
        chargeable.NoCharges.Should().BeTrue();
    }
}
