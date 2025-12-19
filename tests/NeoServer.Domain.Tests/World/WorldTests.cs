using FluentAssertions;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Models.Tiles;
using Xunit;

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
        var location = CreateLocation(x: 500, y: 500, z: 7);
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
        var locationZ8 = CreateLocation(x: 100, y: 100, z: 8);
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
        var location = CreateLocation(x: ushort.MaxValue, y: ushort.MaxValue, z: 15);
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
        var location1 = CreateLocation(x: 100, y: 100, z: 7);
        var location2 = CreateLocation(x: 200, y: 200, z: 7);
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
}
