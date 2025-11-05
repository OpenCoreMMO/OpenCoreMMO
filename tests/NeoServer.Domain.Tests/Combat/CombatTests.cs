using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Combat;

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
    
    [Fact]
    public void Player_consumes_mana_when_attacking_with_magic_weapon()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);
        PlayerCombatService playerCombatService = new(attackService);

        var magicWeapon = ItemTestDataBuilder.CreateMagicWeapon(1, itemTypeAttributes: [(ItemTypeAttribute.ManaUse, 50)]);
        var player = PlayerTestDataBuilder.Build(mana: 100);
        player.Inventory.AddItem(magicWeapon, Slot.Left);
        
        var enemy = MonsterTestDataBuilder.Build();

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        var initialMana = player.Mana;

        //act
        playerCombatService.Attack(player, enemy);

        //assert
        player.Mana.Should().Be(initialMana - 50);
    }
}