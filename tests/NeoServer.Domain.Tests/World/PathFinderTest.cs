using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Algorithms.AStar;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Tests.World;

public class PathFinderTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [ThreadBlocking]
    public void Path_finder_gets_all_directions_to_target(bool fullPathSearch)
    {
        //arrange
        var map = MapTestDataBuilder.Build(32089, 32095, 32202, 32207, 7, 7);

        var player = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[32090, 32202, 7]).AddCreature(player);

        var fpp = new FindPathParams
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            OneStep = false,
            FullPathSearch = fullPathSearch,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1
        };

        var tileEnterRule = PlayerEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, player, player.Location, new Location(32094, 32205, 7), fpp,
            tileEnterRule);

        //assert
        result.Found.Should().BeTrue();
        result.Directions.Should().BeEquivalentTo([
            Direction.East, Direction.South, Direction.East, Direction.South, Direction.East
        ]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [ThreadBlocking]
    public void Path_finder_gets_no_directions_when_closed_to_target(bool fullPathSearch)
    {
        //arrange
        var map = MapTestDataBuilder.Build(32089, 32095, 32202, 32207, 7, 7);

        var player = PlayerTestDataBuilder.Build();

        ((DynamicTile)map[32093, 32204, 7]).AddCreature(player);

        var fpp = new FindPathParams
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            OneStep = false,
            FullPathSearch = fullPathSearch,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1
        };

        var tileEnterRule = PlayerEnterTileRule.Rule;

        //act
        var result = AStar.GetPathMatching(map, player, player.Location, new Location(32094, 32205, 7), fpp,
            tileEnterRule);

        //assert
        result.Found.Should().BeTrue();
        result.Directions.Should().BeEmpty();
    }

    [Fact]
    public void Path_finder_navigates_around_obstacles_to_destination()
    {
        //arrange
        // Create a custom map representing the grid:
        // 00100
        // 02220
        // 02320
        // 00000
        // Where: 0=free, 1=creature start, 2=blocks, 3=destination

        var map = MapTestDataBuilder.Build(100, 104, 100, 103, 7, 7);

        // Set up creature at (102,100) - position marked as '1'
        var player = PlayerTestDataBuilder.Build();
        ((DynamicTile)map[102, 100, 7]).AddCreature(player);

        // Create blocking tiles at positions marked as '2'
        // Row 1: positions (101,101), (102,101), (103,101)
        var blockTile1 = map[101, 101, 7] as DynamicTile;
        var blockTile2 = map[102, 101, 7] as DynamicTile;
        var blockTile3 = map[103, 101, 7] as DynamicTile;

        // Row 2: positions (101,102), (103,102)
        var blockTile4 = map[101, 102, 7] as DynamicTile;
        var blockTile5 = map[103, 102, 7] as DynamicTile;

        // Make these tiles unpassable by setting them as blocking
        if (blockTile1 != null)
        {
            var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagsField.SetValue(blockTile1, (uint)TileFlags.Unpassable);
        }
        if (blockTile2 != null)
        {
            var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagsField.SetValue(blockTile2, (uint)TileFlags.Unpassable);
        }
        if (blockTile3 != null)
        {
            var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagsField.SetValue(blockTile3, (uint)TileFlags.Unpassable);
        }
        if (blockTile4 != null)
        {
            var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagsField.SetValue(blockTile4, (uint)TileFlags.Unpassable);
        }
        if (blockTile5 != null)
        {
            var flagsField = typeof(BaseTile).GetField("Flags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagsField.SetValue(blockTile5, (uint)TileFlags.Unpassable);
        }

        var fpp = new FindPathParams
        {
            AllowDiagonal = true,
            ClearSight = true,
            KeepDistance = false,
            OneStep = false,
            FullPathSearch = true,
            MaxSearchDist = 12,
            MaxTargetDist = 1,
            MinTargetDist = 1
        };

        var tileEnterRule = PlayerEnterTileRule.Rule;

        // Destination at (102,102) - position marked as '3'
        var destination = new Location(102, 102, 7);

        //act
        var result = AStar.GetPathMatching(map, player, player.Location, destination, fpp, tileEnterRule);

        //assert
        result.Found.Should().BeTrue();
        result.Directions.Should().NotBeEmpty();
    }
}