using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

public class WorldTests
{
    private static Domain.World.World CreateWorld()
    {
        return new Domain.World.World();
    }

    private static StaticTile CreateStaticTile(Location location)
    {
        return new StaticTile(location);
    }

    private static DynamicTile CreateDynamicTile(Location location)
    {
        return new DynamicTile(new Coordinate(location.X, location.Y, (sbyte)location.Z), TileFlag.None, null, [], []);
    }

    private static Location CreateLocation(ushort x = 100, ushort y = 100, byte z = 7)
    {
        return new Location(x, y, z);
    }

    private static IMonster CreateMonster(string name = "TestMonster", uint health = 100)
    {
        return MonsterTestDataBuilder.Build(health, name: name);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_replaces_tile_when_new_tile_provided()
    {
        // Arrange
        var world = CreateWorld();
        var location = CreateLocation();
        var originalTile = CreateStaticTile(location);
        var replacementTile = CreateDynamicTile(location);

        world.AddTile(originalTile, location);

        // Act
        world.ReplaceTile(replacementTile);

        // Assert
        world.TryGetTile(ref location, out var retrievedTile).Should().BeTrue();
        retrievedTile.Should().BeSameAs(replacementTile);
        retrievedTile.Should().NotBeSameAs(originalTile);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_creates_sector_when_replacing_tile_in_new_location()
    {
        // Arrange
        var world = CreateWorld();
        var location = CreateLocation(500, 500, 7);
        var tile = CreateDynamicTile(location);

        // Act
        world.ReplaceTile(tile);

        // Assert
        world.TryGetTile(ref location, out var retrievedTile).Should().BeTrue();
        retrievedTile.Should().BeSameAs(tile);
        world.LoadedTilesCount.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_increments_loaded_tiles_count_when_replacing_tile()
    {
        // Arrange
        var world = CreateWorld();
        var location = CreateLocation();
        var originalTile = CreateStaticTile(location);
        var replacementTile = CreateDynamicTile(location);

        world.AddTile(originalTile, location);
        var countBeforeReplace = world.LoadedTilesCount;

        // Act
        world.ReplaceTile(replacementTile);

        // Assert
        world.LoadedTilesCount.Should().Be(countBeforeReplace + 1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void World_replaces_tiles_at_different_z_levels()
    {
        // Arrange
        var world = CreateWorld();
        var locationZ7 = CreateLocation(z: 7);
        var locationZ8 = CreateLocation(100, 100, 8);
        var tileZ7 = CreateStaticTile(locationZ7);
        var tileZ8 = CreateDynamicTile(locationZ8);

        world.AddTile(tileZ7, locationZ7);

        // Act
        world.ReplaceTile(tileZ8);

        // Assert
        world.TryGetTile(ref locationZ7, out var retrievedZ7).Should().BeTrue();
        retrievedZ7.Should().BeSameAs(tileZ7);

        world.TryGetTile(ref locationZ8, out var retrievedZ8).Should().BeTrue();
        retrievedZ8.Should().BeSameAs(tileZ8);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_replaces_dynamic_tile_with_static_tile()
    {
        // Arrange
        var world = CreateWorld();
        var location = CreateLocation();
        var dynamicTile = CreateDynamicTile(location);
        var staticTile = CreateStaticTile(location);

        world.AddTile(dynamicTile, location);

        // Act
        world.ReplaceTile(staticTile);

        // Assert
        world.TryGetTile(ref location, out var retrievedTile).Should().BeTrue();
        retrievedTile.Should().BeSameAs(staticTile);
        retrievedTile.Should().BeOfType<StaticTile>();
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void World_replaces_tile_at_boundary_coordinates()
    {
        // Arrange
        var world = CreateWorld();
        var location = CreateLocation(ushort.MaxValue, ushort.MaxValue, 15);
        var originalTile = CreateStaticTile(location);
        var replacementTile = CreateDynamicTile(location);

        world.AddTile(originalTile, location);

        // Act
        world.ReplaceTile(replacementTile);

        // Assert
        world.TryGetTile(ref location, out var retrievedTile).Should().BeTrue();
        retrievedTile.Should().BeSameAs(replacementTile);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_maintains_separate_tiles_after_replacement()
    {
        // Arrange
        var world = CreateWorld();
        var location1 = CreateLocation(100, 100, 7);
        var location2 = CreateLocation(200, 200, 7);
        var tile1 = CreateStaticTile(location1);
        var tile2 = CreateDynamicTile(location2);
        var replacementTile1 = CreateDynamicTile(location1);

        world.AddTile(tile1, location1);
        world.AddTile(tile2, location2);

        // Act
        world.ReplaceTile(replacementTile1);

        // Assert
        world.TryGetTile(ref location1, out var retrieved1).Should().BeTrue();
        retrieved1.Should().BeSameAs(replacementTile1);

        world.TryGetTile(ref location2, out var retrieved2).Should().BeTrue();
        retrieved2.Should().BeSameAs(tile2);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_swaps_creature_when_moving_between_different_sectors()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        // Sector 1 coordinates (32, 32) - different from Sector 2
        var fromLocation = CreateLocation(32, 32, 7);
        // Sector 2 coordinates (96, 96) - 64+ tiles away to ensure different sector
        var toLocation = CreateLocation(96, 96, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var toSector = world.GetSector(toLocation.X, toLocation.Y);

        fromSector.AddCreature(creature);

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        fromSector.Creatures.Should().NotContain(creature);
        toSector.Creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_does_not_swap_creature_when_moving_within_same_sector()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        // Both locations in same sector (within 32 tile range)
        var fromLocation = CreateLocation(100, 100, 7);
        var toLocation = CreateLocation(110, 110, 7);

        // Create sector by adding tile
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);

        var sector = world.GetSector(fromLocation.X, fromLocation.Y);
        sector.AddCreature(creature);
        var initialCreatureCount = sector.Creatures.Count;

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        sector.Creatures.Should().Contain(creature);
        sector.Creatures.Count.Should().Be(initialCreatureCount);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_removes_creature_from_old_sector_when_swapping()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        var fromLocation = CreateLocation(32, 32, 7);
        var toLocation = CreateLocation(128, 128, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        fromSector.AddCreature(creature);

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        fromSector.Creatures.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_adds_creature_to_new_sector_when_swapping()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        var fromLocation = CreateLocation(32, 32, 7);
        var toLocation = CreateLocation(128, 128, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var toSector = world.GetSector(toLocation.X, toLocation.Y);

        fromSector.AddCreature(creature);
        var initialToSectorCount = toSector.Creatures.Count;

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        toSector.Creatures.Should().Contain(creature);
        toSector.Creatures.Count.Should().Be(initialToSectorCount + 1);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void World_swaps_creature_between_diagonal_sectors()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        var fromLocation = CreateLocation(0, 0, 7);
        var toLocation = CreateLocation(192, 192, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var toSector = world.GetSector(toLocation.X, toLocation.Y);

        fromSector.AddCreature(creature);

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        fromSector.Creatures.Should().NotContain(creature);
        toSector.Creatures.Should().Contain(creature);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void World_swaps_multiple_creatures_between_sectors()
    {
        // Arrange
        var world = CreateWorld();
        var creature1 = CreateMonster("Monster1");
        var creature2 = CreateMonster("Monster2");

        var fromLocation = CreateLocation(50, 50, 7);
        var toLocation = CreateLocation(150, 150, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var toSector = world.GetSector(toLocation.X, toLocation.Y);

        fromSector.AddCreature(creature1);
        fromSector.AddCreature(creature2);

        // Act
        world.SwapCreatureBetweenSectors(creature1, fromLocation, toLocation);
        world.SwapCreatureBetweenSectors(creature2, fromLocation, toLocation);

        // Assert
        fromSector.Creatures.Should().BeEmpty();
        toSector.Creatures.Should().HaveCount(2);
        toSector.Creatures.Should().Contain(creature1);
        toSector.Creatures.Should().Contain(creature2);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void World_swaps_creature_at_sector_boundary()
    {
        // Arrange
        var world = CreateWorld();
        var creature = CreateMonster();

        // Test at sector boundary (32-tile boundary)
        var fromLocation = CreateLocation(31, 31, 7);
        var toLocation = CreateLocation(64, 64, 7);

        // Create sectors by adding tiles
        world.AddTile(CreateStaticTile(fromLocation), fromLocation);
        world.AddTile(CreateStaticTile(toLocation), toLocation);

        var fromSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var toSector = world.GetSector(toLocation.X, toLocation.Y);

        fromSector.AddCreature(creature);

        // Act
        world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);

        // Assert
        fromSector.Creatures.Should().NotContain(creature);
        toSector.Creatures.Should().Contain(creature);
    }
}