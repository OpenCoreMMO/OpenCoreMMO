using System;
using FluentAssertions;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Item;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players.Inventory;

public class InventoryDefenseCalculationTests
{
    [Fact]
    public void Inventory_total_defense_is_the_sum_of_defense_equipment_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Defense, 10)
        });

        var shield = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "shield"),
            (ItemAttribute.Defense, 40)
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
        var legs = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "legs"),
            (ItemAttribute.Armor, 10)
        });

        var helmet = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.BodyPosition, "head"),
            (ItemAttribute.Armor, 40)
        });

        inventory.AddItem(legs);
        inventory.AddItem(helmet);

        //assert
        inventory.TotalArmor.Should().Be(50);
    }
}