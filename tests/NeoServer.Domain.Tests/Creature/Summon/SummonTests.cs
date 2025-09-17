using Moq;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Factories;
using NeoServer.Domain.Creatures.Monster;
using NeoServer.Domain.Creatures.Monster.Combat;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Spawns;
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
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 6, 7, true);
        
        // Create master player A at position (105, 105, 7) - floor 7
        var master = PlayerTestDataBuilder.Build(
            id: 1,
            name: "PlayerA",
            hp: 150);
        
        // Create player B (potential target) at position (106, 105, 6) - floor 6
        var playerB = PlayerTestDataBuilder.Build(
            id: 2,
            name: "PlayerB",
            hp: 150);
        
        // Create summon for master at position (104, 105, 7) - floor 7
        var summon = MonsterTestDataBuilder.BuildSummon(master, 50, 100);
        
        // Place creatures on the map
        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[104, 105, 7] as DynamicTile)?.AddCreature(summon);
        (map[106, 105, 7] as DynamicTile)?.AddCreature(playerB);
        
        // Act
        // Move master to a position that triggers floor change to floor 6
        var success = map.TryMoveCreature(master, new Location(105, 105, 6));
        
        summon.SetAsEnemy(playerB);
        
        // Assert
        // The summon should not automatically attack player B just because master changed floors
        // The summon only attacks when the master has a target
        summon.Attacking.Should().BeFalse();
        summon.AutoAttackTargetId.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_distance_monster_follows_master_closely_when_no_target()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);

        var master = PlayerTestDataBuilder.Build();

        var monsterType = new MonsterType
        {
            Name = "Monster X",
            MaxHealth = 100,
            Speed = 100,
            TargetChance = new IntervalChance(1000, 50),
            Attacks =
            [
                new MonsterCombatType
                {
                    Interval = 0,
                    AttackChance = 100,
                    CombatParameter = new CombatParameter
                    {
                        MinDamage = 10,
                        MaxDamage = 100,
                        DamageType = DamageType.Melee
                    }
                }
            ]
        };

        monsterType.Flags.Add(CreatureFlagAttribute.Hostile, 1);
        monsterType.Flags.Add(CreatureFlagAttribute.TargetDistance, 4);

        var summon = new NeoServer.Domain.Creatures.Monster.Summon.Summon(monsterType, mapTool, master);

        // Place creatures on the map
        (map[105, 105, 7] as DynamicTile)?.AddCreature(master);
        (map[109, 105, 7] as DynamicTile)?.AddCreature(summon);

        // Act
        // Set summon to follow master (no target scenario)
        summon.Follow(master);

        // Assert
        var pathParams = summon.PathSearchParams;
        pathParams.MaxTargetDist.Should().Be(1, "Summon should try to get close to master when following");
        pathParams.KeepDistance.Should().BeFalse("Summon should not keep distance from master");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void Summon_distance_monster_keeps_distance_from_enemy_when_attacking()
    {
        // Arrange
        var master = PlayerTestDataBuilder.Build();
        var enemy = PlayerTestDataBuilder.Build();

        // Create a distance monster summon (TargetDistance = 4)
        var summon = (NeoServer.Domain.Creatures.Monster.Summon.Summon)MonsterTestDataBuilder.BuildSummon(master, targetDistance: 4);

        // Set master to have a target
        master.SetAttackTarget(enemy);

        // Act
        // Update summon state to attack the master's target
        summon.UpdateState();

        // Assert
        var pathParams = summon.PathSearchParams;
        pathParams.MaxTargetDist.Should().Be(4, "Summon should keep its TargetDistance when attacking enemy");
        pathParams.KeepDistance.Should().BeTrue("Summon should keep distance from enemy");
    }

    [Fact]
    [Trait("Category", "Summon")]
    public void SummonService_does_not_summon_on_unpassable_tile()
    {
        // Arrange
        var ground = MapTestDataBuilder.CreateGround(new Location(100, 100, 7));
        var masterTile = new DynamicTile(new Coordinate(100, 100, 7), (TileFlag)TileFlags.None, ground, null, null);
        var unpassableTile = new DynamicTile(new Coordinate(101, 100, 7), (TileFlag)TileFlags.Unpassable, ground, null, null);
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
}