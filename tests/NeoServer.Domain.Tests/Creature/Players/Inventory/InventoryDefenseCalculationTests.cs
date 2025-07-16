using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players.Inventory;

public class InventoryDefenseCalculationTests
{
    [Fact]
    public void Inventory_total_defense_is_the_sum_of_defense_equipment()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Defense, 10)
            ]);

        var shield = ItemTestData.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "shield"),
                (ItemTypeAttribute.Defense, 40)
            ]);

        inventory.AddItem(weapon);
        inventory.AddItem(shield, (byte)Slot.Right);

        //assert
        inventory.TotalDefense.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_armor_is_the_sum_of_armor_value_equipment()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var legs = ItemTestData.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "legs"),
                (ItemTypeAttribute.Armor, 10)
            ]);

        var helmet = ItemTestData.CreateDefenseEquipmentItem(1, 
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "head"),
                (ItemTypeAttribute.Armor, 40)
            ]);

        inventory.AddItem(legs);
        inventory.AddItem(helmet);

        //assert
        inventory.TotalArmor.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_defense_is_the_sum_of_defense_equipment_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Defense, 10)
            ],
            itemAttributes:
            [
                (ItemAttribute.Defense, 20)
            ]);

        var shield = ItemTestData.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "shield"),
                (ItemTypeAttribute.Defense, 40)
            ],
            itemAttributes:
            [
                (ItemAttribute.Defense, 80)
            ]);

        inventory.AddItem(weapon);
        inventory.AddItem(shield, (byte)Slot.Right);

        //assert
        inventory.TotalDefense.Should().Be(100);
    }

    [Fact]
    public void Inventory_total_armor_is_the_sum_of_armor_value_equipment_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var legs = ItemTestData.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "legs"),
                (ItemTypeAttribute.Armor, 10)
            ],
            itemAttributes:
            [
                (ItemAttribute.Armor, 20)
            ]);

        var helmet = ItemTestData.CreateDefenseEquipmentItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.BodyPosition, "head"),
                (ItemTypeAttribute.Armor, 40)
            ],
            itemAttributes:
            [
                (ItemAttribute.Armor, 80)
            ]);

        inventory.AddItem(legs);
        inventory.AddItem(helmet);

        //assert
        inventory.TotalArmor.Should().Be(100);
    }
}