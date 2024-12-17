using System;
using FluentAssertions;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players.Inventory;

public class InventoryAttackCalculationTests
{
    [Fact]
    public void Inventory_total_attack_is_the_melee_weapon_attack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 50)
        });

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_attack_is_the_distance_weapon_extra_attack_plus_ammo_extra_attack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateDistanceWeapon(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 50)
        });

        var ammo = ItemTestData.CreateAmmo(1, 50, new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 60)
        });

        inventory.AddItem(weapon);
        inventory.AddItem(ammo);

        //assert
        inventory.TotalAttack.Should().Be(110);
    }

    [Fact]
    public void Inventory_total_attack_is_the_throwable_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 50)
        });

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(50);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateDistanceWeapon(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Range, 30)
        });

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(30);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_throwable_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateThrowableDistanceItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Range, 30)
        });

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(30);
    }

    [Fact]
    public void Inventory_attack_range_is_0_when_no_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestData.CreateWeaponItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.Attack, 30)
        });

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(0);
    }
}