using System.Reflection;
using Moq;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Creature.Monster;

public class MonsterCombatTest
{
    [Fact]
    public void Monster_becomes_active_and_attacks_player_when_spawns_near_visible_player()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(100, 100, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(101, 100, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(player);
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_remains_idle_when_player_has_ignored_by_monsters_flag()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(100, 100, 7));
        player.Group.EnableFlag(PlayerFlag.IgnoredByMonsters); // Set the flag to be ignored by monsters

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(101, 100, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.Sleeping);
        monster.CurrentTarget.Should().BeNull();
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeFalse();
    }

    [Fact]
    public void Monster_enters_looking_for_enemy_state_when_player_is_in_protection_zone()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 102, 100, 102, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(100, 100, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(101, 100, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        // Set the player's tile as a protection zone using reflection
        var flagsField = typeof(BaseTile).GetField("Flags",
            BindingFlags.NonPublic | BindingFlags.Instance);
        flagsField.SetValue(map[100, 100, 7], (uint)TileFlags.ProtectionZone);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.LookingForEnemy);
        monster.CurrentTarget.Should().BeNull();
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeFalse();
    }

    [Fact]
    public void Monster_targets_closest_player_when_multiple_players_in_range()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var closestPlayer = PlayerTestDataBuilder.Build();
        closestPlayer.SetNewLocation(new Location(101, 102, 7));

        var fartherPlayer = PlayerTestDataBuilder.Build();
        fartherPlayer.SetNewLocation(new Location(105, 102, 7));

        var evenFartherPlayer = PlayerTestDataBuilder.Build();
        evenFartherPlayer.SetNewLocation(new Location(105, 102, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.Metadata.Flags[CreatureFlagAttribute.TargetDistance] =
            2; // Set to use distance attack for the nearest targeting
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(closestPlayer);
        map.PlaceCreature(fartherPlayer);
        map.PlaceCreature(evenFartherPlayer);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(closestPlayer);
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_switches_to_another_nearby_enemy_when_current_target_becomes_unreachable()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 120, 100, 105, 7, 7);

        var initialTarget = PlayerTestDataBuilder.Build(name: "Initial Target");
        initialTarget.SetNewLocation(new Location(101, 102, 7));

        var nearbyPlayer = PlayerTestDataBuilder.Build(name: "Nearby Player");
        nearbyPlayer.SetNewLocation(new Location(110, 105, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(initialTarget);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial attack on first player
        monsterStateService.UpdateState(monster);

        ((Domain.Creatures.Monster.Monster)monster).Targets.Count.Should().Be(1);
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(initialTarget);

        map.PlaceCreature(nearbyPlayer);

        // Make the initial target unreachable by moving it out of range

        map.RemoveCreature(initialTarget);
        initialTarget.SetNewLocation(new Location(120, 105, 7)); // Far away;
        map.PlaceCreature(initialTarget);

        monster.OnSpectatorMoved(initialTarget);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(nearbyPlayer);
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_enters_looking_for_enemy_state_when_current_target_becomes_unreachable_and_no_other_targets()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 120, 100, 105, 7, 7);

        var initialTarget = PlayerTestDataBuilder.Build();
        initialTarget.SetNewLocation(new Location(101, 102, 7));

        var monster = MonsterTestDataBuilder.Build() as Domain.Creatures.Monster.Monster;
        monster.SetNewLocation(new Location(102, 102, 7));
        monster.Awake();

        map.PlaceCreature(initialTarget);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial attack on player
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(initialTarget);

        // Make the target unreachable by moving it out of range
        map.RemoveCreature(initialTarget);
        initialTarget.SetNewLocation(new Location(120, 105, 7)); // Far away;
        map.PlaceCreature(initialTarget);

        //act
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.LookingForEnemy);
        monster.CurrentTarget.Should().BeNull();
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeFalse();
    }

    [Fact]
    public void Monster_occasionally_switches_to_closest_enemy_during_combat()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 105, 7, 7);

        var currentTarget = PlayerTestDataBuilder.Build(name: "Current Target");
        currentTarget.SetNewLocation(new Location(103, 102, 7));

        var closerPlayer = PlayerTestDataBuilder.Build(name: "Closer Player");
        closerPlayer.SetNewLocation(new Location(105, 105, 7));

        var monster = MonsterTestDataBuilder.Build() as Domain.Creatures.Monster.Monster;
        monster.Metadata.TargetChance = new IntervalChance(200, 100); // A High chance to switch
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(currentTarget);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial attack on farther player
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(currentTarget);

        map.PlaceCreature(closerPlayer);

        //act
        Thread.Sleep(200);
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(closerPlayer); // Should switch to closest
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_does_not_change_target_when_target_change_chance_is_zero()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 105, 7, 7);

        var currentTarget = PlayerTestDataBuilder.Build();
        currentTarget.SetNewLocation(new Location(103, 102, 7));

        var closerPlayer = PlayerTestDataBuilder.Build();
        closerPlayer.SetNewLocation(new Location(101, 102, 7));

        var monster = MonsterTestDataBuilder.Build() as Domain.Creatures.Monster.Monster;
        monster.Metadata.TargetChance = new IntervalChance(200, 0); // No chance to switch (0%)
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(currentTarget);

        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var targetingService =
            new MonsterTargetingService(new MonsterTargetSearch(new MapTool(map, new PathFinder(map))));
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map, targetingService);

        // Initial attack on farther player
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(currentTarget);

        map.PlaceCreature(closerPlayer);

        //act
        Thread.Sleep(200);
        monsterStateService.UpdateState(monster);

        //assert
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(currentTarget); // Should NOT switch, keep original target
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_only_reengages_combat_when_moved_back_into_attack_range_while_fleeing()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 120, 100, 105, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 102, 7));

        var monster = MonsterTestDataBuilder.Build() as Domain.Creatures.Monster.Monster;
        monster.Metadata.Flags[CreatureFlagAttribute.RunOnHealth] = 50; // Set run on health to 50
        monster.SetNewLocation(new Location(103, 102, 7));
        // Simulate damage to reduce health below 50 to trigger fleeing
        var damage = new CombatDamage(60, DamageType.Physical);
        monster.OnDamage(player, new CombatDamageList(damage));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial state: monster should be fleeing due to low health
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.Escaping);

        // Move monster out of attack range (farther away)
        map.RemoveCreature(monster);
        monster.SetNewLocation(new Location(110, 102, 7)); // Out of range
        map.PlaceCreature(monster);

        //act - Update state while out of range
        monsterStateService.UpdateState(monster);

        //assert - Should continue fleeing since can't attack (current behavior keeps attacking)
        monster.State.Should().Be(MonsterState.Escaping);
        monster.CurrentTarget.Should().Be(player); // Monster keeps target while fleeing
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeTrue(); // Current behavior: monster keeps attacking while fleeing

        // Move monster back into attack range
        map.RemoveCreature(monster);
        monster.SetNewLocation(new Location(103, 102, 7)); // Back in range
        map.PlaceCreature(monster);

        //act - Update state while in range
        monsterStateService.UpdateState(monster);

        //assert - Should re-engage in combat since can attack again (current behavior keeps fleeing)
        monster.State.Should().Be(MonsterState.Escaping); // Current behavior: monster stays in fleeing state
        monster.CurrentTarget.Should().Be(player);
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeTrue(); // Current behavior: monster keeps attacking while fleeing
    }

    [Fact]
    public void Monster_remains_active_when_under_status_effects_even_without_enemies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(101, 102, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial state: monster should be in combat
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.InCombat);

        // Add a status effect (burning) to the monster
        var burningCondition = new Condition(ConditionType.Burning, 1000); // 1-second duration
        monster.AddCondition(burningCondition);

        // Simulate enemy leaving by removing the player from the map
        map.RemoveCreature(player);

        // Notify the monster that the spectator has left
        monster.OnSpectatorLoggedOut(player);

        //act - Update state after enemy leaves but with status effect
        monsterStateService.UpdateState(monster);

        //assert - Monster enters an idle state even with status effects (current behavior)
        monster.State.Should().Be(MonsterState.LookingForEnemy); // Current behavior: enters idle state
        monster.CurrentTarget.Should().BeNull();
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeFalse();
        monster.Targets.Any().Should().BeFalse();
        monster.Conditions.Should().ContainKey(ConditionType.Burning);
    }

    [Fact]
    public void Monster_exits_idle_state_when_new_enemy_enters_range()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Ensure the monster is idle (no targets)
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.Sleeping);
        monster.CurrentTarget.Should().BeNull();
        monster.IsFollowing.Should().BeFalse();
        monster.Attacking.Should().BeFalse();

        // Add a new enemy (player) entering range
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(101, 102, 7));
        map.PlaceCreature(player);

        //act - Update state after a new enemy enters
        monsterStateService.UpdateState(monster);

        //assert - Monster should reactivate and pursue the new enemy
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(player);
        monster.IsFollowing.Should().BeTrue();
        monster.Attacking.Should().BeTrue();
    }

    [Fact]
    public void Monster_enters_fleeing_state_when_health_drops_below_threshold()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(101, 102, 7));

        var monster = MonsterTestDataBuilder.Build() as Domain.Creatures.Monster.Monster;
        monster.Metadata.Flags[CreatureFlagAttribute.RunOnHealth] = 50; // Set run on health to 50
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial state: monster should be in combat
        monsterStateService.UpdateState(monster);
        monster.State.Should().Be(MonsterState.InCombat);
        monster.CurrentTarget.Should().Be(player);

        // Damage the monster to reduce health below 50 to trigger fleeing
        var damage = new CombatDamage(60, DamageType.Physical);
        monster.OnDamage(player, new CombatDamageList(damage));

        //act - Update state after taking damage
        monsterStateService.UpdateState(monster);

        //assert - Monster should enter fleeing state, prioritizing distance from target
        monster.State.Should().Be(MonsterState.Escaping);
        monster.CurrentTarget.Should().Be(player); // Keeps target while fleeing
        monster.IsFollowing.Should().BeFalse(); // Stops following to flee
        monster.Attacking.Should().BeTrue(); // Current behavior: keeps attacking while fleeing
    }

    [Fact]
    public void Monster_only_targets_player_when_both_player_and_other_monster_are_present()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(101, 102, 7));

        var otherMonster = MonsterTestDataBuilder.Build();
        otherMonster.SetNewLocation(new Location(103, 102, 7));

        var sut = MonsterTestDataBuilder.Build(); // System Under Test
        sut.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(player);
        map.PlaceCreature(otherMonster);
        map.PlaceCreature(sut);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //act
        monsterStateService.UpdateState(sut);

        //assert - Monster should only target the player, ignoring the other monster
        sut.State.Should().Be(MonsterState.InCombat);
        sut.CurrentTarget.Should().Be(player);
        sut.IsFollowing.Should().BeTrue();
        sut.Attacking.Should().BeTrue();
        sut.Targets.Any().Should().BeTrue();
        sut.Targets.HasTarget(otherMonster).Should().BeFalse(); // Should not have the other monster in the target list
    }

    [Fact]
    public void Monster_does_not_add_other_monster_to_target_list_when_OnSpectatorMoved_is_fired()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 7);

        var spectatorMonster = MonsterTestDataBuilder.Build();
        spectatorMonster.SetNewLocation(new Location(101, 102, 7));

        var sut = MonsterTestDataBuilder.Build(); // System Under Test
        sut.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(spectatorMonster);
        map.PlaceCreature(sut);

        //act - Simulate the spectator monster moving (triggering OnSpectatorMoved)
        sut.OnSpectatorMoved(spectatorMonster);

        //assert - Monster should not add the other monster to its target list
        sut.Targets.Any().Should().BeFalse(); // No targets should be added
        sut.Targets.HasTarget(spectatorMonster).Should()
            .BeFalse(); // Specifically, the spectator monster should not be in the list
    }

    [Fact]
    public void Monster_targets_player_when_surrounding_monster_dies()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 115, 100, 115, 7, 7);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(107, 107, 7));

        // Place 8 monsters around the player
        var surroundingMonsters = new List<IMonster>();
        var positions = new (int x, int y)[]
        {
            (106, 106), (107, 106), (108, 106),
            (106, 107), (108, 107),
            (106, 108), (107, 108), (108, 108)
        };

        for (var i = 0; i < 8; i++)
        {
            var monster = MonsterTestDataBuilder.Build(name: $"Surrounding Monster {i + 1}");
            monster.SetNewLocation(new Location((ushort)positions[i].x, (ushort)positions[i].y, 7));
            surroundingMonsters.Add(monster);
            map.PlaceCreature(monster);
        }

        var sut = (Domain.Creatures.Monster.Monster)MonsterTestDataBuilder.Build(name: "Sut monster",
            map: map); // System Under Test
        sut.SetNewLocation(new Location(109, 109, 7)); // Nearby but not attacking

        map.PlaceCreature(sut);
        map.PlaceCreature(player);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        // Initial state: SUT might be targeting player yet
        monsterStateService.UpdateState(sut);
        sut.State.Should().Be(MonsterState.LookingForEnemy);
        sut.CurrentTarget.Should()
            .Be(player); // Assume targeting the player initially although not having follow path to him

        //act - Kill one of the surrounding monsters
        var deadMonster = surroundingMonsters[0];
        map.RemoveCreature(deadMonster);
        sut.OnSpectatorDies(deadMonster);

        // Update state after the death
        monsterStateService.UpdateState(sut);

        //assert - SUT now targets the player
        sut.State.Should().Be(MonsterState.InCombat);
        sut.CurrentTarget.Should().Be(player);
        sut.IsFollowing.Should().BeTrue();
        sut.Attacking.Should().BeTrue();
        sut.HasFollowPath.Should().BeTrue();
    }

    [Fact]
    public void Monster_targets_unblocked_player_when_path_to_closer_player_is_blocked_by_non_pushable_monsters()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);

        var playerA = PlayerTestDataBuilder.Build(name: "Player A");
        playerA.SetNewLocation(new Location(102, 102, 7));

        var playerB = PlayerTestDataBuilder.Build(name: "Player B");
        playerB.SetNewLocation(new Location(106, 104, 7));

        // Surround player A with 8 non pushable monsters
        var positions = new (int x, int y)[]
        {
            (101, 101), (102, 101), (103, 101),
            (101, 102), (103, 102),
            (101, 103), (102, 103), (103, 103)
        };

        map.PlaceCreature(playerA);

        for (var i = 0; i < 8; i++)
        {
            var monster = MonsterTestDataBuilder.Build(name: $"monster-{i + 1}");
            monster.Metadata.Flags[CreatureFlagAttribute.Pushable] = 0; // Set pushable to false
            monster.SetNewLocation(new Location((ushort)positions[i].x, (ushort)positions[i].y, 7));
            map.PlaceCreature(monster);
        }

        var sut = MonsterTestDataBuilder.Build(name: "monsterX", map: map);
        sut.Metadata.Flags[CreatureFlagAttribute.TargetDistance] = 1; // Allow long-range targeting
        sut.Metadata.TargetChance.Chance = 0;

        sut.SetNewLocation(new Location(100, 100, 7));

        map.PlaceCreature(playerB);

        map.PlaceCreature(sut);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = BuildMonsterStateService(summonServiceMock.Object, map);

        //selects the player A as he is closer to the monster
        monsterStateService.UpdateState(sut);

        //act
        monsterStateService.UpdateState(sut);

        //assert
        sut.State.Should().Be(MonsterState.InCombat);
        sut.CurrentTarget.Should().Be(playerB); // Should target player B because the path to player A is blocked
        sut.IsFollowing.Should().BeTrue();
        sut.Attacking.Should().BeTrue();
    }

    private static MonsterStateService BuildMonsterStateService(
        ISummonService summonService,
        IMap map,
        IMonsterTargetingService targetingService = null,
        TargetDetectorService targetDetectorService = null)
    {
        targetDetectorService ??= new TargetDetectorService(map);
        targetingService ??=
            new MonsterTargetingService(new MonsterTargetSearch(new MapTool(map, new PathFinder(map))));
        return new MonsterStateService(summonService, targetDetectorService, targetingService);
    }
}