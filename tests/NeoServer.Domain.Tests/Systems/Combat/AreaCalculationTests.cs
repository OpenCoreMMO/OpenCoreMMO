using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.Systems.Combat;

public class AreaCalculationTests
{
    [Fact]
    public void Area_with_walkable_tile_should_be_affected()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        var tile = MapTestDataBuilder.CreateTile(targetLocation);
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Single(result.Locations);
        Assert.Equal(targetLocation, result.Locations[0]);
    }

    [Fact]
    public void Area_with_protection_zone_tile_should_not_be_affected()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        // Create a tile with protection zone flag
        var ground = MapTestDataBuilder.CreateGround(targetLocation);
        var tile = new DynamicTile(new Coordinate(targetLocation), TileFlag.None, ground, [], []);
        
        // Use reflection to set the flag since SetFlag is protected
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(tile, (uint)TileFlags.ProtectionZone);
        
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Empty(result.Locations);
    }

    [Fact]
    public void Area_with_hole_tile_should_not_be_affected()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        // Create a tile with hole by using floor change down attribute
        var ground = MapTestDataBuilder.CreateGround(targetLocation);
        ground.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");
        var tile = new DynamicTile(new Coordinate(targetLocation), TileFlag.None, ground, 
            [], []);
        
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Empty(result.Locations);
    }

    [Fact]
    public void Area_with_block_missile_tile_should_not_be_affected()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        // Create a tile with block projectile flag
        var ground = MapTestDataBuilder.CreateGround(targetLocation);
        var tile = new DynamicTile(new Coordinate(targetLocation), TileFlag.None, ground, 
            [], []);
        
        // Use reflection to set the flag since SetFlag is protected
        var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        flagsField.SetValue(tile, (uint)TileFlags.BlockProjecTile);
        
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Empty(result.Locations);
    }

    [Fact]
    public void Area_with_creatures_on_tile_should_include_creatures()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        var player = PlayerTestDataBuilder.Build();
        var creatures = new List<IWalkableCreature> { player };

        // Create a tile with creatures
        var ground = MapTestDataBuilder.CreateGround(targetLocation);
        var tile = new DynamicTile(new Coordinate(targetLocation), TileFlag.None, ground, 
            [], []);
        
        // Add creatures to the tile
        tile.AddCreature(player);
        
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Single(result.Locations);
        Assert.Single(result.Creatures);
        Assert.Equal(player, result.Creatures[0]);
    }

    [Fact]
    public void Area_with_no_creatures_on_tile_should_not_include_creatures()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var targetLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        var tile = MapTestDataBuilder.CreateTile(targetLocation);
        
        var map = MapTestDataBuilder.Build(tile);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Single(result.Locations);
        Assert.Empty(result.Creatures);
    }

    [Fact]
    public void Area_with_multiple_coordinates_should_return_multiple_locations()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var area = new[]
        {
            new Coordinate(100, 100, 7),
            new Coordinate(101, 100, 7),
            new Coordinate(100, 101, 7)
        };

        // Create tiles for each coordinate
        var tile1 = MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2 = MapTestDataBuilder.CreateTile(new Location(101, 100, 7));
        var tile3 = MapTestDataBuilder.CreateTile(new Location(100, 101, 7));
        
        var map = MapTestDataBuilder.Build(tile1, tile2, tile3);
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Equal(3, result.Locations.Count);
    }

    [Fact]
    public void Area_with_empty_coordinate_array_should_return_empty_results()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var area = Array.Empty<Coordinate>();
        
        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        Assert.Empty(result.Locations);
        Assert.Empty(result.Creatures);
    }

    [Fact]
    public void Area_with_null_map_tile_should_return_coordinates()
    {
        // Arrange
        var originLocation = new Location(100, 100, 7);
        var area = new[] { new Coordinate(100, 100, 7) };

        var map = MapTestDataBuilder.Build(Array.Empty<ITile>());
        
        var service = new AreaCalculationService(map);

        // Act
        var result = service.CalculateAffectedTargets(originLocation, area);

        // Assert
        result.Locations.Should().HaveCount(1);
    }
}