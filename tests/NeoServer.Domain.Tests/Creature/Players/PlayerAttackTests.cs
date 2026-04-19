using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Creature.Players;

public class PlayerAttackTests
{
    [Fact]
    public void Player_cannot_attack_target_when_enemy_is_in_protection_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var protectionZoneTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.ProtectionZone,
            ground, null, null);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        protectionZoneTile.AddCreature(enemy);
        regularTile.AddCreature(player);

        using var monitor = player.Monitor();

        var map = MapTestDataBuilder.Build(regularTile, protectionZoneTile);
        var attackService = AttackServiceTestBuilder.Build(map);

        //act
        player.SetAttackTarget(enemy);
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CannotAttackPersonInProtectionZone);

        monitor.Should().Raise(nameof(player.OnStoppedAttack));
        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_set_attack_target_while_in_protection_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var protectionZoneTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.ProtectionZone,
            ground, null, null);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        protectionZoneTile.AddCreature(player);
        regularTile.AddCreature(enemy);

        using var monitor = player.Monitor();

        var map = MapTestDataBuilder.Build(regularTile, protectionZoneTile);
        var attackService = AttackServiceTestBuilder.Build(map);

        //act

        player.SetAttackTarget(enemy);

        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));


        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CannotAttackWhileInProtectionZone);

        monitor.Should().Raise(nameof(player.OnStoppedAttack));
        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_attack_when_enemy_goes_to_protection_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var regularTile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var protectionZoneTile = new DynamicTile(new Coordinate(101, 100, 7), (TileFlag)TileFlags.ProtectionZone,
            ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, regularTile2, protectionZoneTile);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        regularTile2.AddCreature(enemy);

        using var monitor = player.Monitor();

        player.SetAttackTarget(enemy);

        regularTile2.RemoveCreature(enemy, out _);
        protectionZoneTile.AddCreature(enemy);

        //act

        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Failed.Should().BeTrue();

        monitor.Should().Raise(nameof(player.OnStoppedAttack));

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_attack_when_goes_to_protection_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var regularTile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var protectionZoneTile = new DynamicTile(new Coordinate(101, 100, 7), (TileFlag)TileFlags.ProtectionZone,
            ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, regularTile2, protectionZoneTile);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        regularTile2.AddCreature(enemy);

        using var monitor = player.Monitor();

        player.SetAttackTarget(enemy);

        regularTile.RemoveCreature(player, out _);
        protectionZoneTile.AddCreature(player);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CannotAttackWhileInProtectionZone);

        monitor.Should().Raise(nameof(player.OnStoppedAttack));

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_does_not_get_logout_block_on_protection_zone_tile()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var protectionZoneTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.ProtectionZone,
            ground, null, null);

        var map = MapTestDataBuilder.Build(protectionZoneTile);
        var player = (NeoServer.Domain.Creatures.Player.Player)PlayerTestDataBuilder.Build(map: map);

        protectionZoneTile.AddCreature(player);

        //act
        player.SetLogoutBlock();

        //assert
        player.IsLogoutBlocked.Should().BeFalse();
        player.CannotLogout.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_attack_dead_enemy()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build(hp: 0);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        using var monitor = player.Monitor();

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CreatureIsDead);
    }

    [Fact]
    public void Player_cannot_attack_enemy_in_another_floor()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var secondFloor = new DynamicTile(new Coordinate(100, 101, 6), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, secondFloor);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        secondFloor.AddCreature(enemy);

        using var monitor = player.Monitor();

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_stops_attack_when_enemy_goes_to_another_floor()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var regularTile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var secondFloor = new DynamicTile(new Coordinate(100, 101, 6), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, regularTile2, secondFloor);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(attackSpeed: 1);
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        regularTile2.AddCreature(enemy);

        using var monitor = player.Monitor();

        player.SetAttackTarget(enemy);

        attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        regularTile2.RemoveCreature(enemy, out _);
        secondFloor.AddCreature(enemy);

        //act
        Thread.Sleep(100);
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);

        monitor.Should().Raise(nameof(player.OnStoppedAttack));

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_attack_when_enemy_is_a_floor_below()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var secondFloor = new DynamicTile(new Coordinate(100, 101, 8), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, secondFloor);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        secondFloor.AddCreature(enemy);

        using var monitor = player.Monitor();

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }

    [Fact]
    public void Player_cannot_attack_when_enemy_is_too_far()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var regularTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var secondFloor = new DynamicTile(new Coordinate(100, 150, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(regularTile, secondFloor);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        regularTile.AddCreature(player);
        secondFloor.AddCreature(enemy);

        using var monitor = player.Monitor();

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);

        player.Attacking.Should().BeFalse();
        player.CurrentTarget.Should().BeNull();
        player.AutoAttackTargetId.Should().Be(0);
        player.IsFollowing.Should().BeFalse();
    }
}
