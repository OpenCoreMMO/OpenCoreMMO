using FluentAssertions;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Creatures.Player.Inventory;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Combat;

public class CombatTests
{
    [Fact]
    public void Player_Gets_More_Damage_When_Has_Damage_Percentage_Increased()
    {
        //arrange
        var victim = PlayerTestDataBuilder.Build(hp: 1000);
        var attacker = PlayerTestDataBuilder.Build();
        
        //act
        victim.ReceiveAttack(attacker, new CombatDamage(100, DamageType.Physical));
        
        //assert
        victim.HealthPoints.Should().Be(900);
        
        //act
        victim.IncreaseDamageReceived(100);
        victim.ReceiveAttack(attacker, new CombatDamage(100, DamageType.Physical));
        
        //assert
        victim.HealthPoints.Should().Be(700);
        
        //act
        victim.DecreaseDamageReceived(100);
        victim.ReceiveAttack(attacker, new CombatDamage(100, DamageType.Physical));
        
        //assert
        victim.HealthPoints.Should().Be(600);
    }
    
    [Fact]
    public void Player_Does_Not_Block_Attack_When_Shield_Defense_Is_Disabled()
    {
        //arrange
        var inventory = InventoryTestDataBuilder.Build();
        var shield = ItemTestData.CreateBodyEquipmentItem(7, "", "shield");
        shield.Metadata.Attributes.SetAttribute(ItemAttribute.Defense, byte.MaxValue);
        
        inventory.AddItem(shield, Slot.Right);
        
        var victim = PlayerTestDataBuilder.Build(hp: 1000, capacity: uint.MaxValue );
        victim.AddInventory(inventory);
        
        var attacker = PlayerTestDataBuilder.Build();
        
        //act
        victim.ReceiveAttack(attacker, new CombatDamage(1, DamageType.Melee));

        //assert
        victim.HealthPoints.Should().Be(1000);
        
        //act
        victim.DisableShieldDefense();
        victim.ReceiveAttack(attacker, new CombatDamage(1, DamageType.Melee));

        //assert
        victim.HealthPoints.Should().Be(999);
        
        //act
        victim.EnableShieldDefense();
        victim.ReceiveAttack(attacker, new CombatDamage(1, DamageType.Melee));
        
        //assert
        victim.HealthPoints.Should().Be(999);
    }
}