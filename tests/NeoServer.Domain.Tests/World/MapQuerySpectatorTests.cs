using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Map;

namespace NeoServer.Domain.Tests.World;

public class MapQuerySpectatorTests
{
    private static Map CreateMapWithCreatures(params (Location location, ICreature creature)[] creatures)
    {
        var map = (Map)MapTestDataBuilder.Build(50, 200, 50, 200, 7, 9);

        foreach (var (location, creature) in creatures)
        {
            creature.SetNewLocation(location);
            map.PlaceCreature(creature);
        }

        return map;
    }

    private static Location CreateLocation(ushort x = 100, ushort y = 100, byte z = 7)
    {
        return new Location(x, y, z);
    }

    private static IMonster CreateMonster(string name = "Monster", Location? location = null)
    {
        var loc = location ?? CreateLocation();
        var monster = MonsterTestDataBuilder.Build(name: name);
        monster.SetNewLocation(loc);
        return monster;
    }

    private static IPlayer CreatePlayer(string name = "Player", Location? location = null)
    {
        var loc = location ?? CreateLocation();
        var player = PlayerTestDataBuilder.Build(name: name);
        player.SetNewLocation(loc);
        return player;
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_spectators_when_locations_are_near()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var creature = CreateMonster(location: CreateLocation(102, 102, 7));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_only_players_when_onlyPlayer_is_true()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var player = CreatePlayer(location: CreateLocation(102, 102, 7));
        var monster = CreateMonster(location: CreateLocation(103, 103, 7));

        var map = CreateMapWithCreatures((player.Location, player), (monster.Location, monster));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation, true);

        // Assert
        spectators.Should().Contain(player);
        spectators.Should().NotContain(monster);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_all_creatures_when_onlyPlayer_is_false()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var player = CreatePlayer(location: CreateLocation(102, 102, 7));
        var monster = CreateMonster(location: CreateLocation(103, 103, 7));

        var map = CreateMapWithCreatures((player.Location, player), (monster.Location, monster));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation, false);

        // Assert
        spectators.Should().Contain(player);
        spectators.Should().Contain(monster);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_empty_set_when_no_creatures_in_range()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var creature = CreateMonster(location: CreateLocation(150, 150, 7));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_spectators_from_both_locations_when_far_apart()
    {
        // Arrange - locations more than MaxViewPort apart (11 tiles)
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(130, 130, 7);
        var creature1 = CreateMonster("Monster1", CreateLocation(102, 102, 7));
        var creature2 = CreateMonster("Monster2", CreateLocation(128, 128, 7));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature1);
        spectators.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_spectators_when_locations_on_same_floor()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var creature = CreateMonster(location: CreateLocation(102, 102, 7));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_spectators_when_locations_on_different_floors()
    {
        // Arrange - different floors should use union of both location spectators
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(100, 100, 8);
        var creature1 = CreateMonster("Monster1", CreateLocation(102, 102, 7));
        var creature2 = CreateMonster("Monster2", CreateLocation(102, 102, 8));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature1);
        spectators.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_unique_spectators_when_creature_visible_from_both_locations()
    {
        // Arrange - locations far apart but creature visible from both
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(130, 130, 7); // 30 tiles away (far apart)
        var creature1 = CreateMonster("Monster1", CreateLocation(105, 105, 7)); // Near first location
        var creature2 = CreateMonster("Monster2", CreateLocation(125, 125, 7)); // Near second location

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert - should get creatures from both locations combined
        spectators.Should().HaveCount(2);
        spectators.Should().Contain(creature1);
        spectators.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_spectators_at_exact_viewport_boundary()
    {
        // Arrange - MaxViewPortX/Y is 11
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(111, 100, 7); // Exactly 11 tiles away
        var creature = CreateMonster(location: CreateLocation(105, 100, 7));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_handles_multiple_creatures_in_spectator_range()
    {
        // Arrange
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(105, 105, 7);
        var creatures = new ICreature[]
        {
            CreateMonster("Monster1", CreateLocation(101, 101, 7)),
            CreateMonster("Monster2", CreateLocation(102, 102, 7)),
            CreateMonster("Monster3", CreateLocation(103, 103, 7)),
            CreateMonster("Monster4", CreateLocation(104, 104, 7))
        };

        var creatureTuples = creatures.Select(c => (c.Location, c)).ToArray();
        var map = CreateMapWithCreatures(creatureTuples);

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().HaveCount(4);
        foreach (var creature in creatures) spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_adjusts_range_when_fromLocation_north_of_toLocation()
    {
        // Arrange - from.Y > to.Y means moving north
        var fromLocation = CreateLocation(100, 105);
        var toLocation = CreateLocation();
        var creature = CreateMonster(location: CreateLocation(100, 103));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_adjusts_range_when_fromLocation_west_of_toLocation()
    {
        // Arrange - from.X > to.X means moving west
        var fromLocation = CreateLocation(105);
        var toLocation = CreateLocation();
        var creature = CreateMonster(location: CreateLocation(103));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_spectators_when_locations_are_identical()
    {
        // Arrange
        var location = CreateLocation();
        var creature = CreateMonster(location: CreateLocation(102, 102));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var spectators = map.GetSpectators(location, location);

        // Assert
        spectators.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_filters_players_correctly_in_mixed_creature_group()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(105, 105);
        var player1 = CreatePlayer("Player1", CreateLocation(101, 101));
        var player2 = CreatePlayer("Player2", CreateLocation(102, 102));
        var monster1 = CreateMonster("Monster1", CreateLocation(103, 103));
        var monster2 = CreateMonster("Monster2", CreateLocation(104, 104));

        var map = CreateMapWithCreatures(
            (player1.Location, player1),
            (player2.Location, player2),
            (monster1.Location, monster1),
            (monster2.Location, monster2)
        );

        // Act
        var spectators = map.GetSpectators(fromLocation, toLocation, true);

        // Assert
        spectators.Should().HaveCount(2);
        spectators.Should().Contain(player1);
        spectators.Should().Contain(player2);
        spectators.Should().NotContain(monster1);
        spectators.Should().NotContain(monster2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_creatures_at_single_location_when_both_locations_are_same()
    {
        // Arrange
        var location = CreateLocation();
        var creature = CreateMonster(location: CreateLocation(102, 102));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(location, location);

        // Assert
        creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_creatures_from_both_zones_when_locations_differ()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(130, 130);
        var creature1 = CreateMonster("Monster1", CreateLocation(102, 102));
        var creature2 = CreateMonster("Monster2", CreateLocation(128, 128));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(creature1);
        creatures.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_empty_when_no_creatures_in_zone()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(105, 105);
        var creature = CreateMonster(location: CreateLocation(150, 150)); // Far away

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_both_players_and_monsters_in_zone()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(105, 105);
        var player = CreatePlayer(location: CreateLocation(102, 102));
        var monster = CreateMonster(location: CreateLocation(103, 103));

        var map = CreateMapWithCreatures((player.Location, player), (monster.Location, monster));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(player);
        creatures.Should().Contain(monster);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_unique_creatures_when_visible_from_both_zones()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(130, 130);
        var creature1 = CreateMonster("Monster1", CreateLocation(105, 105));
        var creature2 = CreateMonster("Monster2", CreateLocation(125, 125));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert - should have unique creatures only
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(creature1);
        creatures.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_creatures_from_different_floors_in_zone()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(100, 100, 8);
        var creature1 = CreateMonster("Monster1", CreateLocation(102, 102));
        var creature2 = CreateMonster("Monster2", CreateLocation(102, 102, 8));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(creature1);
        creatures.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_creatures_when_zones_far_apart()
    {
        // Arrange - Very far locations
        var fromLocation = CreateLocation(50, 50);
        var toLocation = CreateLocation(180, 180);
        var creature1 = CreateMonster("Monster1", CreateLocation(55, 55));
        var creature2 = CreateMonster("Monster2", CreateLocation(175, 175));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(creature1);
        creatures.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_multiple_creatures_from_same_zone()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(105, 105);
        var creatures = new ICreature[]
        {
            CreateMonster("Monster1", CreateLocation(101, 101)),
            CreateMonster("Monster2", CreateLocation(102, 102)),
            CreateMonster("Monster3", CreateLocation(103, 103)),
            CreatePlayer("Player1", CreateLocation(104, 104))
        };

        var creatureTuples = creatures.Select(c => (c.Location, c)).ToArray();
        var map = CreateMapWithCreatures(creatureTuples);

        // Act
        var result = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        result.Should().HaveCount(4);
        foreach (var creature in creatures) result.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_creatures_only_from_first_zone_when_second_zone_empty()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(180, 180);
        var creature = CreateMonster(location: CreateLocation(102, 102));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(1);
        creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_returns_creatures_only_from_second_zone_when_first_zone_empty()
    {
        // Arrange
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(180, 180);
        var creature = CreateMonster(location: CreateLocation(178, 178));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(1);
        creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Map_returns_creatures_at_zone_boundary()
    {
        // Arrange - Creatures at viewport boundary
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(111); // Exactly 11 tiles away
        var creature = CreateMonster(location: CreateLocation(105));

        var map = CreateMapWithCreatures((creature.Location, creature));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Map_combines_creatures_from_adjacent_zones()
    {
        // Arrange - Adjacent but separate zones
        var fromLocation = CreateLocation();
        var toLocation = CreateLocation(115, 115);
        var creature1 = CreateMonster("Monster1", CreateLocation(102, 102));
        var creature2 = CreateMonster("Monster2", CreateLocation(113, 113));

        var map = CreateMapWithCreatures((creature1.Location, creature1), (creature2.Location, creature2));

        // Act
        var creatures = map.GetCreaturesAtPositionZone(fromLocation, toLocation);

        // Assert
        creatures.Should().HaveCount(2);
        creatures.Should().Contain(creature1);
        creatures.Should().Contain(creature2);
    }
}