using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players.Inventory;

public class InventoryDefenseCalculationTests
{
    [Fact]
    public void Inventory_total_defense_is_the_sum_of_defense_equipment_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.Defense, 10)
        });

        var shield = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.BodyPosition, "shield"),
            (ItemTypeAttribute.Defense, 40)
        });

        inventory.AddItem(weapon);
        inventory.AddItem(shield, (byte)Slot.Right);

        //assert
        inventory.TotalDefense.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_armor_is_the_sum_of_armor_value_equipment_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var legs = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.BodyPosition, "legs"),
            (ItemTypeAttribute.Armor, 10)
        });

        var helmet = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemTypeAttribute, IConvertible)[]
        {
            (ItemTypeAttribute.BodyPosition, "head"),
            (ItemTypeAttribute.Armor, 40)
        });

        inventory.AddItem(legs);
        inventory.AddItem(helmet);

        //assert
        inventory.TotalArmor.Should().Be(50);
    }
}