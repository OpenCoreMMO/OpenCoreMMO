using Moq;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Monster.Services;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;
using TileFlags = NeoServer.Domain.Common.Location.TileFlags;

namespace NeoServer.Domain.Tests.Creature.Summon;

public class SummonTests
{
    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_does_not_attack_player_B_when_master_player_A_has_no_target_and_moves_to_another_floor()
    {
        // Arrange
        // Create a map with multiple floors to test floor change
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 6, 7);

        // Create master player A at position (105, 105, 7) - floor 7
        var master = PlayerTestDataBuilder.Build(hp: 150);

        // Create player B (potential target) at position (106, 105, 6) - floor 6
        var playerB = PlayerTestDataBuilder.Build(
            2,
            "PlayerB",
            hp: 150);

        // Create summon for master at position (104, 105, 7) - floor 7
        var summon = MonsterTestDataBuilder.BuildSummon(master, 50);

        // Place creatures on the map
        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[104, 105, 7] as DynamicTile)?.AddCreature(summon);
        (map[106, 105, 7] as DynamicTile)?.AddCreature(playerB);

        var creatureMovementService =
            new CreatureMovementService(map, new CylinderOperation(map), new CreatureMovementValidation(map));

        // Act
        // Move master to a position that triggers floor change to floor 6
        var success = creatureMovementService.MoveCreature(master, new Location(105, 105, 6));

        summon.SetAsEnemy(playerB);

        // Assert
        // The summon should not automatically attack player B just because master changed floors
        // The summon only attacks when the master has a target
        summon.Attacking.Should().BeFalse();
        summon.AutoAttackTargetId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void SummonService_does_not_summon_on_unpassable_tile()
    {
        // Arrange
        var ground = MapTestDataBuilder.CreateGround(new Location(100, 100, 7));
        var masterTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var unpassableTile =
            new DynamicTile(new Coordinate(101, 100, 7), (TileFlag)TileFlags.Unpassable, ground, null, null);
        var map = MapTestDataBuilder.Build(masterTile, unpassableTile);

        var master = PlayerTestDataBuilder.Build();
        masterTile.AddCreature(master);

        var summonToBeCreated = MonsterTestDataBuilder.BuildSummon(master);

        var creatureFactory = new Mock<ICreatureFactory>();
        creatureFactory.Setup(x => x.CreateSummon(It.IsAny<string>(), master)).Returns(summonToBeCreated);

        // Create summon service
        var summonService = new SummonService(creatureFactory.Object, map, null);

        // Act
        var summon = summonService.SpamSummon(master, "TestSummon");

        // Assert
        summon.Should().BeNull("Summon should not be created on unpassable tile");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_disappears_when_master_moves_2_floors_up()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 5, 7);
        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[104, 105, 7] as DynamicTile)?.AddCreature(summon);
        var creatureMovementService =
            new CreatureMovementService(map, new CylinderOperation(map), new CreatureMovementValidation(map));

        // Act
        // Move master 2 floors up (from 7 to 5)
        creatureMovementService.MoveCreature(master, new Location(105, 105, 5));
        summon.UpdateState();

        // Assert
        master.Summons.Should().NotContain(summon as Domain.Creatures.Monster.Summon.Summon,
            "Summon should be dismissed when master moves 2 floors up");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_disappears_when_master_moves_2_floors_down()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 9);
        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[104, 105, 7] as DynamicTile)?.AddCreature(summon);
        var creatureMovementService =
            new CreatureMovementService(map, new CylinderOperation(map), new CreatureMovementValidation(map));

        // Act
        // Move master 2 floors down (from 7 to 9)
        creatureMovementService.MoveCreature(master, new Location(105, 105, 9));
        summon.UpdateState();

        // Assert
        master.Summons.Should().NotContain(summon as Domain.Creatures.Monster.Summon.Summon,
            "Summon should be dismissed when master moves 2 floors down");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_disappears_when_master_moves_more_than_40_sqms_away()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 150, 100, 150, 7, 7);
        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[141, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(summon);

        // Act
        summon.UpdateState();

        // Assert
        master.Summons.Should().NotContain(summon as Domain.Creatures.Monster.Summon.Summon,
            "Summon should be dismissed when master is more than 40 sqms away");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_does_not_disappear_when_master_is_within_range()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 150, 100, 150, 7, 7);
        var master = PlayerTestDataBuilder.Build();
        var summon = MonsterTestDataBuilder.BuildSummon(master);

        (map[140, 100, 7] as DynamicTile)?.AddCreature(master);
        (map[100, 100, 7] as DynamicTile)?.AddCreature(summon);

        // Act
        summon.UpdateState();

        // Assert
        master.Summons.Should().Contain(summon as Domain.Creatures.Monster.Summon.Summon,
            "Summon should not be dismissed when master is within 40 sqms and same floor");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_does_not_attack_master_when_set_as_enemy()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        
        var master = PlayerTestDataBuilder.Build();
        master.SetNewLocation(new Location(105, 105, 7));
        
        var summon = MonsterTestDataBuilder.BuildSummon(master);
        summon.SetNewLocation(new Location(104, 105, 7));

        map.PlaceCreature(master);
        map.PlaceCreature(summon);

        var summonServiceMock = new Mock<ISummonService>();
        var targetDetectorService = new TargetDetectorService(map);
        var pathFinder = new PathFinder(map);
        var monsterTargetingService = new MonsterTargetingService(new MonsterTargetSearch(new MapTool(map, pathFinder)));
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, targetDetectorService, monsterTargetingService);

        // Act
        monsterStateService.UpdateState(summon);
        summon.SetAttackTarget(master);

        // Assert
        summon.Targets.HasTarget(master).Should().BeFalse("Summon should never add its master as a target");
        summon.CurrentTarget.Should().NotBe(master, "Summon should never target its master");
        summon.Attacking.Should().BeFalse("Summon should not be attacking when trying to attack master");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_looking_for_enemy_does_not_attack_floor7_creatures_when_master_has_target_on_floor8()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 8);

        // Master will be on floor 8
        var master = PlayerTestDataBuilder.Build();
        master.SetNewLocation(new Location(105, 105, 8));

        // Summon and other creatures on floor 7
        var summon = MonsterTestDataBuilder.BuildSummon(master);
        summon.SetNewLocation(new Location(104, 105, 7));

        var monsterX = PlayerTestDataBuilder.Build(2, "MonsterX");
        monsterX.SetNewLocation(new Location(106, 105, 7));

        var playerY = PlayerTestDataBuilder.Build(3, "PlayerY");
        playerY.SetNewLocation(new Location(107, 105, 7));

        // Place creatures on the map
        map.PlaceCreature(master);
        map.PlaceCreature(summon);
        map.PlaceCreature(monsterX);
        map.PlaceCreature(playerY);

        // Give the master a target on floor 8 (different creature)
        var targetOnZ8 = PlayerTestDataBuilder.Build(4, "TargetZ8");
        targetOnZ8.SetNewLocation(new Location(108, 108, 8));
        map.PlaceCreature(targetOnZ8);
        master.SetAttackTarget(targetOnZ8);

        // Prepare services used to update monster/summon state
        var summonServiceMock = new Mock<ISummonService>();
        var targetDetectorService = new TargetDetectorService(map);
        var pathFinder = new PathFinder(map);
        var monsterTargetingService = new MonsterTargetingService(new MonsterTargetSearch(new MapTool(map, pathFinder)));
        var monsterStateService = new MonsterStateService(summonServiceMock.Object, targetDetectorService, monsterTargetingService);

        // Act
        monsterStateService.UpdateState(summon);

        // Assert
        // Summon should not be attacking or have an auto-attack target just because master's target is on another floor
        summon.State.Should().Be(MonsterState.RandomlyWalking);
        summon.Attacking.Should().BeFalse();
        summon.AutoAttackTargetId.Should().Be(0);

        // Summon should not acquire targets that are on floor 7 (nearby creatures) when master is on floor 8
        summon.Targets.HasTarget(monsterX).Should().BeFalse("Summon must not target nearby floor-7 monster when its master is on a different floor");
        summon.Targets.HasTarget(playerY).Should().BeFalse("Summon must not target nearby floor-7 player when its master is on a different floor");
    }
}