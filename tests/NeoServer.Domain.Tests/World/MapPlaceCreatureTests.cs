using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

/// <summary>
/// Unit tests for the Map.PlaceCreature method.
/// Tests cover: tile validation, creature entry rules, neighbor tile search,
/// sector registration, and event dispatching.
/// </summary>
public class MapPlaceCreatureTests
{
    private static Map CreateMapWithMockEventAggregator(int fromX, int toX, int fromY, int toY, int fromZ, int toZ,
        Mock<IEventAggregator> mockEventAggregator)
    {
        var world = new Domain.World.World();
        var map = new Map(world, mockEventAggregator.Object);

        for (var x = fromX; x <= toX; x++)
        for (var y = fromY; y <= toY; y++)
        for (var z = fromZ; z <= toZ; z++)
        {
            var ground = MapTestDataBuilder.CreateGround(new Location((ushort)x, (ushort)y, (byte)z));
            world.AddTile(new DynamicTile(new Coordinate(x, y, (sbyte)z), TileFlag.None, ground, [], null),
                new Location((ushort)x, (ushort)y, (byte)z));
        }

        return map;
    }

    #region Happy Path Tests

    [Fact]
    public void PlaceCreature_WhenTileIsEmpty_ShouldPlaceCreatureOnTile()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(50, 52, 50, 52, 7, 7);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(51, 51, 7));

        // Act
        map.PlaceCreature(player);

        // Assert
        var tile = map[51, 51, 7] as IDynamicTile;
        tile.Should().NotBeNull();
        tile!.HasCreature(player).Should().BeTrue();
    }

    [Fact]
    public void PlaceCreature_ShouldInvokeCreatureAddedOnMapEvent()
    {
        // Arrange
        var mockEventAggregator = new Mock<IEventAggregator>();
        var map = CreateMapWithMockEventAggregator(50, 52, 50, 52, 7, 7, mockEventAggregator);
        
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(51, 51, 7));

        // Act
        map.PlaceCreature(player);

        // Assert
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.Is<CreatureAddedOnMapEvent>(e => 
                e.Creature == player && e.Cylinder != null)),
            Times.Once);
    }

    #endregion

    #region Tile Validation Tests

    [Fact]
    public void PlaceCreature_WhenTileIsNull_ShouldNotPlaceCreature()
    {
        // Arrange - Create map with limited tiles
        var mockEventAggregator = new Mock<IEventAggregator>();
        var map = CreateMapWithMockEventAggregator(50, 52, 50, 52, 7, 7, mockEventAggregator);
        var player = PlayerTestDataBuilder.Build();
        
        // Set location to a tile that doesn't exist
        player.SetNewLocation(new Location(200, 200, 7));

        // Act
        map.PlaceCreature(player);

        // Assert - Event should not be invoked
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<CreatureAddedOnMapEvent>()),
            Times.Never);
    }

    #endregion

    #region Creature Already On Tile Tests

    [Fact]
    public void PlaceCreature_WhenSameCreatureAlreadyOnTile_ShouldNotFireEventAgain()
    {
        // Arrange
        var mockEventAggregator = new Mock<IEventAggregator>();
        var map = CreateMapWithMockEventAggregator(50, 52, 50, 52, 7, 7, mockEventAggregator);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(51, 51, 7));

        // First placement
        map.PlaceCreature(player);
        mockEventAggregator.Invocations.Clear();

        // Act - Second placement of same creature
        map.PlaceCreature(player);

        // Assert - Event should not fire again
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<CreatureAddedOnMapEvent>()),
            Times.Never);
    }

    [Fact]
    public void PlaceCreature_WhenDifferentCreatureOnTile_ShouldPlaceOnNeighbourTile()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(50, 54, 50, 54, 7, 7);
        
        var player1 = PlayerTestDataBuilder.Build(id: 1, name: "Player1");
        player1.SetNewLocation(new Location(52, 52, 7));
        map.PlaceCreature(player1);

        var player2 = PlayerTestDataBuilder.Build(id: 2, name: "Player2");
        player2.SetNewLocation(new Location(52, 52, 7)); // Same location as player1

        // Act
        map.PlaceCreature(player2);

        // Assert - Player2 should be on a neighbour tile, not on the same tile as player1
        var originalTile = map[52, 52, 7] as IDynamicTile;
        originalTile!.HasCreature(player1).Should().BeTrue();
        
        // Player2 should be placed somewhere (on a neighbour)
        var player2Placed = false;
        foreach (var neighbour in originalTile.Location.Neighbours)
        {
            if (map[neighbour] is IDynamicTile neighbourTile && neighbourTile.HasCreature(player2))
            {
                player2Placed = true;
                break;
            }
        }
        player2Placed.Should().BeTrue();
    }

    #endregion

    #region Event Behavior Tests

    [Fact]
    public void PlaceCreature_WhenNonWalkableCreature_ShouldNotInvokeCreatureAddedOnMapEvent()
    {
        // Arrange
        var mockEventAggregator = new Mock<IEventAggregator>();
        var map = CreateMapWithMockEventAggregator(50, 52, 50, 52, 7, 7, mockEventAggregator);
        
        var creatureMock = new Mock<ICreature>();
        creatureMock.Setup(c => c.Location).Returns(new Location(51, 51, 7));
        creatureMock.Setup(c => c.CreatureId).Returns(999);

        // Act
        map.PlaceCreature(creatureMock.Object);

        // Assert - CreatureAddedOnMapEvent only fires for IWalkableCreature
        mockEventAggregator.Verify(
            ea => ea.InvokeEvent(It.IsAny<CreatureAddedOnMapEvent>()),
            Times.Never);
    }

    #endregion

    #region Multiple Creatures Tests

    [Fact]
    public void PlaceCreature_WithMultipleCreatures_ShouldPlaceEachOnDifferentTiles()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(50, 55, 50, 55, 7, 7);

        var player1 = PlayerTestDataBuilder.Build(id: 1, name: "Player1");
        var player2 = PlayerTestDataBuilder.Build(id: 2, name: "Player2");
        var player3 = PlayerTestDataBuilder.Build(id: 3, name: "Player3");

        player1.SetNewLocation(new Location(52, 52, 7));
        player2.SetNewLocation(new Location(52, 52, 7));
        player3.SetNewLocation(new Location(52, 52, 7));

        // Act
        map.PlaceCreature(player1);
        map.PlaceCreature(player2);
        map.PlaceCreature(player3);

        // Assert - All three should be placed on different tiles
        var tilesWithCreatures = new HashSet<Location>();
        
        for (ushort x = 50; x <= 55; x++)
        for (ushort y = 50; y <= 55; y++)
        {
            if (map[x, y, 7] is IDynamicTile tile)
            {
                if (tile.HasCreature(player1)) tilesWithCreatures.Add(new Location(x, y, 7));
                if (tile.HasCreature(player2)) tilesWithCreatures.Add(new Location(x, y, 7));
                if (tile.HasCreature(player3)) tilesWithCreatures.Add(new Location(x, y, 7));
            }
        }

        tilesWithCreatures.Count.Should().Be(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void PlaceCreature_WhenAllNeighbourTilesOccupied_ShouldStillAttemptPlacement()
    {
        // Arrange - Small map where center tile and all neighbours can be occupied
        var map = MapTestDataBuilder.Build(50, 54, 50, 54, 7, 7);
        var centerLocation = new Location(52, 52, 7);
        
        // Occupy center tile
        var player1 = PlayerTestDataBuilder.Build(id: 1, name: "Center");
        player1.SetNewLocation(centerLocation);
        map.PlaceCreature(player1);
        
        // Occupy all neighbours
        var id = 2;
        var centerTile = map[centerLocation] as IDynamicTile;
        foreach (var neighbour in centerTile!.Location.Neighbours)
        {
            if (map[neighbour] is IDynamicTile)
            {
                var occupant = PlayerTestDataBuilder.Build(id: (uint)id++, name: $"Neighbour{id}");
                occupant.SetNewLocation(neighbour);
                map.PlaceCreature(occupant);
            }
        }
        
        // Now try to place another creature at the center
        var newPlayer = PlayerTestDataBuilder.Build(id: 100, name: "NewPlayer");
        newPlayer.SetNewLocation(centerLocation);
        
        // Act
        map.PlaceCreature(newPlayer);

        // Assert - The method should still execute without crashing
        // Whether placement succeeds depends on finding any empty neighbour
        // This test mainly ensures no exception is thrown
    }

    #endregion
}

