using Moq;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(map[100, 100, 7], (uint)TileFlags.ProtectionZone);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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

        var initialTarget = PlayerTestDataBuilder.Build(name:"Initial Target");
        initialTarget.SetNewLocation(new Location(101, 102, 7));
        
        var nearbyPlayer = PlayerTestDataBuilder.Build(name: "Nearby Player");
        nearbyPlayer.SetNewLocation(new Location(110, 105, 7));

        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(102, 102, 7));

        map.PlaceCreature(initialTarget);
        map.PlaceCreature(monster);

        var summonServiceMock = new Mock<ISummonService>();
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, new TargetDetectorService(map));

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
}