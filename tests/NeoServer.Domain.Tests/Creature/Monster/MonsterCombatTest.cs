using Moq;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures.Monster;
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
        player.SetNewLocation(new Location(100,100,7));
        
        var monster = MonsterTestDataBuilder.Build();
        monster.SetNewLocation(new Location(101,100,7));

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
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
}