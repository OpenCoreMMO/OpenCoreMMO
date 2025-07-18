using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Player;

namespace NeoServer.Domain.Tests.Creature.Combat;

public class CombatTests
{
    [Fact]
    public void Player_Gets_More_Damage_When_Has_Damage_Percentage_Increased()
    {
        //arrange
        var victim = PlayerTestDataBuilder.Build(hp: 1000);
        var attacker = PlayerTestDataBuilder.Build();

        //act
        victim.TakeDamage(attacker, new CombatDamage(100, DamageType.Physical));

        //assert
        victim.HealthPoints.Should().Be(900);

        //act
        victim.IncreaseDamageReceived(100);
        victim.TakeDamage(attacker, new CombatDamage(100, DamageType.Physical));

        //assert
        victim.HealthPoints.Should().Be(700);

        //act
        victim.DecreaseDamageReceived(100);
        victim.TakeDamage(attacker, new CombatDamage(100, DamageType.Physical));

        //assert
        victim.HealthPoints.Should().Be(600);
    }

    [Fact]
    public void Player_Does_Not_Block_Attack_When_Shield_Defense_Is_Disabled()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var shield = ItemTestDataBuilder.CreateBodyEquipmentItem(7, "", "shield");
        shield.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Defense, byte.MaxValue);

        inventory.AddItem(shield, Slot.Right);

        var victim = PlayerTestDataBuilder.Build(hp: 1000, capacity: uint.MaxValue);
        victim.AddInventory(inventory);

        var attacker = PlayerTestDataBuilder.Build();

        //act
        victim.TakeDamage(attacker, new CombatDamage(1, DamageType.Melee));

        //assert
        victim.HealthPoints.Should().Be(1000);

        //act
        victim.DisableShieldDefense();
        victim.TakeDamage(attacker, new CombatDamage(1, DamageType.Melee));

        //assert
        victim.HealthPoints.Should().Be(999);

        //act
        victim.EnableShieldDefense();
        victim.TakeDamage(attacker, new CombatDamage(1, DamageType.Melee));

        //assert
        victim.HealthPoints.Should().Be(999);
    }
}