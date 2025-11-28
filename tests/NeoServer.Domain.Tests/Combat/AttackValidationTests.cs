using NeoServer.Domain.Combat;
using NeoServer.Domain.Combat.Player;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Helpers.Services;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Combat;

public class AttackValidationTests
{
    [Fact]
    public void Attack_fails_when_aggressor_is_null()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var map = MapTestDataBuilder.Build(tile);
        var attackService = AttackServiceTestBuilder.Build(map);

        var enemy = PlayerTestDataBuilder.Build();
        tile.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(null, enemy, PlayerCombatParameterBuilder.Build(null, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.NotPossible);
    }

    [Fact]
    public void Attack_fails_when_player_cannot_attack_player_due_to_flag()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(groupId: 1);
        player.Group.Flags[PlayerFlag.CannotAttackPlayer] = true;
        var enemy = PlayerTestDataBuilder.Build();

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisPlayer);
    }

    [Fact]
    public void Attack_fails_when_player_is_protected_from_attacking_player()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(level: 1); // Low level
        var enemy = PlayerTestDataBuilder.Build(level: 20); // Higher level

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisPlayer);
    }

    [Fact]
    public void Attack_fails_when_player_attacks_player_in_no_pvp_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.NoPvpZone, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.NoPvpZone, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);
        var enemy = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.NotPermittedInNoPvpZone);
    }

    [Fact]
    public void Attack_fails_when_summon_master_cannot_attack_player()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var master = PlayerTestDataBuilder.Build(groupId: 1);
        master.Group.Flags[PlayerFlag.CannotAttackPlayer] = true;
        var summon = MonsterTestDataBuilder.BuildSummon(master);
        var enemy = PlayerTestDataBuilder.Build();

        tile1.AddCreature(summon);
        tile2.AddCreature(enemy);

        //act
        var result = attackService.Execute(new AttackInput(summon, enemy, new CombatParameter()));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisPlayer);
    }

    [Fact]
    public void Attack_fails_when_player_cannot_attack_monster_due_to_flag()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(groupId: 1);
        player.Group.Flags[PlayerFlag.CannotAttackMonster] = true;
        var enemy = MonsterTestDataBuilder.Build();

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisCreature);
    }

    [Fact]
    public void Attack_fails_when_player_attacks_summon_in_no_pvp_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.NoPvpZone, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.NoPvpZone, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = MonsterTestDataBuilder.BuildSummon(PlayerTestDataBuilder.Build());

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.NotPermittedInNoPvpZone);
    }

    [Fact]
    public void Attack_fails_when_monster_attacks_summon_of_monster()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var monster = MonsterTestDataBuilder.Build();
        var enemy = MonsterTestDataBuilder.BuildSummon(MonsterTestDataBuilder.Build());

        tile1.AddCreature(monster);
        tile2.AddCreature(enemy);

        //act
        var result = attackService.Execute(new AttackInput(monster, enemy, new CombatParameter()));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisCreature);
    }

    [Fact]
    public void Attack_fails_in_optional_pvp_when_not_in_pvp_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map, PvpType.OptionalPvP);

        var player = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);
        var enemy = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisPlayer);
    }

    [Fact]
    public void Attack_succeeds_when_no_target()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var map = MapTestDataBuilder.Build(tile);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        tile.AddCreature(player);

        //act
        var result =
            attackService.Execute(new AttackInput(player, null, PlayerCombatParameterBuilder.Build(player, null)));

        //assert
        result.Result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Attack_fails_when_cannot_see_target()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 150, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(level: 30);
        var enemy = PlayerTestDataBuilder.Build(level: 30);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);
    }

    [Fact]
    public void Attack_fails_when_different_floor()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 6), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(level: 20);
        var enemy = PlayerTestDataBuilder.Build(level: 20);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.TargetLost);
    }

    [Fact]
    public void Attack_fails_when_target_is_dead()
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

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CreatureIsDead);
    }

    [Fact]
    public void Attack_fails_when_target_in_protection_zone()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.ProtectionZone, ground, null,
            null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.CannotAttackPersonInProtectionZone);
    }

    [Fact]
    public void Attack_succeeds_in_valid_conditions()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var player = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);
        var enemy = PlayerTestDataBuilder.Build(experience: 1000, vocationType: 1);

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Player_attacks_own_summon_when_can_attack_summon_is_enabled()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService =
            AttackServiceTestBuilder.Build(map, combatConfig: new CombatConfiguration(false, false, true));

        var player = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(player);

        tile1.AddCreature(player);
        tile2.AddCreature(summon);

        //act
        var result =
            attackService.Execute(new AttackInput(player, summon, PlayerCombatParameterBuilder.Build(player, summon)));

        //assert
        result.Result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Player_fails_to_attack_own_summon_when_can_attack_summon_is_disabled()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map, combatConfig: new CombatConfiguration());

        var player = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(player);

        tile1.AddCreature(player);
        tile2.AddCreature(summon);

        //act
        var result =
            attackService.Execute(new AttackInput(player, summon, PlayerCombatParameterBuilder.Build(player, summon)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.YouMayNotAttackThisCreature);
    }

    [Fact]
    public void Attack_fails_when_player_has_not_enough_mana_for_magic_weapon()
    {
        //arrange
        var location = new Location(100, 100, 7);
        var ground = MapTestDataBuilder.CreateGround(location);

        var tile1 = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var tile2 = new DynamicTile(new Coordinate(100, 101, 7), (TileFlag)TileFlags.None, ground, null, null);

        var map = MapTestDataBuilder.Build(tile1, tile2);
        var attackService = AttackServiceTestBuilder.Build(map);

        var magicWeapon =
            ItemTestDataBuilder.CreateMagicWeapon(1, itemTypeAttributes: [(ItemTypeAttribute.ManaUse, 50)]);
        var player = PlayerTestDataBuilder.Build(mana: 30);
        player.Inventory.AddItem(magicWeapon, Slot.Left);

        var enemy = MonsterTestDataBuilder.Build();

        tile1.AddCreature(player);
        tile2.AddCreature(enemy);

        //act
        var result =
            attackService.Execute(new AttackInput(player, enemy, PlayerCombatParameterBuilder.Build(player, enemy)));

        //assert
        result.Result.Reason.Should().Be(InvalidOperation.NotEnoughMana);
    }
}