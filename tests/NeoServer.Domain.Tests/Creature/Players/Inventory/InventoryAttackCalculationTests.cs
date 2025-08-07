using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Players.Inventory;

public class InventoryAttackCalculationTests
{
    [Fact]
    public void Inventory_total_attack_is_the_melee_weapon_attack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_attack_is_the_distance_weapon_extra_attack_plus_ammo_extra_attack()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ]);

        var ammo = ItemTestDataBuilder.CreateAmmo(1, 50,
        [
            (ItemTypeAttribute.Attack, 60)
        ]);

        inventory.AddItem(weapon);
        inventory.AddItem(ammo);

        //assert
        inventory.TotalAttack.Should().Be(110);
    }

    public static IDictionary<T, IConvertible> Attributes<T>(params (T, IConvertible)[] values)
        where T : Enum
    {
        return values.ToDictionary(x => x.Item1, x => x.Item2);
    }

    [Fact]
    public void Inventory_total_attack_is_the_throwable_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(50);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Range, 30)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(30);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_throwable_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Range, 30)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(30);
    }

    [Fact]
    public void Inventory_attack_range_is_0_when_no_distance_weapon()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 30)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(0);
    }

    [Fact]
    public void Inventory_total_attack_is_the_melee_weapon_attack_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(50);
    }

    [Fact]
    public void Inventory_total_attack_is_the_distance_weapon_extra_attack_plus_ammo_extra_attack_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ],
            itemAttributes:
            [
                (ItemAttribute.Attack, 100)
            ]);

        var ammo = ItemTestDataBuilder.CreateAmmo(1, 50,
            [
                (ItemTypeAttribute.Attack, 60)
            ],
            [
                (ItemAttribute.Attack, 120)
            ]);

        inventory.AddItem(weapon);
        inventory.AddItem(ammo);

        //assert
        inventory.TotalAttack.Should().Be(220);
    }

    [Fact]
    public void Inventory_total_attack_is_the_throwable_distance_weapon_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 50)
            ],
            itemAttributes:
            [
                (ItemAttribute.Attack, 100)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.TotalAttack.Should().Be(100);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_distance_weapon_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateDistanceWeapon(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Range, 30)
            ],
            itemAttributes:
            [
                (ItemAttribute.ShootRange, 60)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(60);
    }

    [Fact]
    public void Inventory_attack_range_is_the_range_of_throwable_distance_weapon_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateThrowableDistanceItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Range, 30)
            ],
            itemAttributes:
            [
                (ItemAttribute.ShootRange, 60)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(60);
    }

    [Fact]
    public void Inventory_attack_range_is_0_when_no_distance_weapon_with_item_attribute()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var weapon = ItemTestDataBuilder.CreateWeaponItem(1,
            itemTypeAttributes:
            [
                (ItemTypeAttribute.Attack, 30)
            ],
            itemAttributes:
            [
                (ItemAttribute.Attack, 60)
            ]);

        inventory.AddItem(weapon);

        //assert
        inventory.AttackRange.Should().Be(0);
    }
}