using System;
using System.Threading.Tasks;
using FluentAssertions;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Item;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Server;
using NeoServer.Game.World.Models.Tiles;
using NeoServer.Modules.Creatures.Events;
using xRetry;
using Xunit;

namespace NeoServer.Game.Tests.Creature.Monster;

public class MonsterWalkTest
{
    [RetryFact(100)]
    public void Monster_that_has_CanPushItems_flag_ignores_objects_in_the_way()
    {
        //arrange

        // --------------------------------------------------------------------------
        // |  Tile 1 (Monster is here) |  Tile 2 (object is here)  |  Tile 3 (Goal) |
        // --------------------------------------------------------------------------

        var itemType = new ItemType
        {
            Flags = { ItemFlag.BlockPathFind }
        };

        var item = new Game.Item.Bases.Item(itemType, new Location(101, 100, 7));

        var destinationTile = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None,
            MapTestDataBuilder.CreateGround(new Location(102, 100, 7)), Array.Empty<IItem>(), null);

        var tiles = new ITile[]
        {
            new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(100, 100, 7)), Array.Empty<IItem>(), null),
            new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(101, 100, 7)), Array.Empty<IItem>(), new IItem[] { item }),
            destinationTile
        };

        var map = MapTestDataBuilder.Build(tiles);

        var sut = MonsterTestDataBuilder.Build(speed: 6000, map: map);
        sut.SetNewLocation(new Location(100, 100, 7));

        sut.Metadata.Flags.Add(CreatureFlagAttribute.CanPushItems, 1);

        var gameServer = GameServerTestBuilder.Build(map);
        var cancellationToken = ServerTestHelper.StartThreads(gameServer);

        sut.OnStartedWalking += new CreatureStartedWalkingEventHandler(gameServer).Execute;

        gameServer.Open();
        map.PlaceCreature(sut);

        //act
        sut.WalkTo(new Location(103, 100, 7));

        Task.Delay(2_000, cancellationToken).Wait(cancellationToken);

        //assert
        sut.Tile.Should().Be(destinationTile);
    }

    [Fact]
    public void Monster_without_can_push_items_flag_do_not_walk()
    {
        //arrange

        // --------------------------------------------------------------------------
        // |  Tile 1 (Monster is here) |  Tile 2 (object is here)  |  Tile 3 (Goal) |
        // --------------------------------------------------------------------------

        var itemType = new ItemType
        {
            Flags = { ItemFlag.BlockPathFind }
        };
        var item = new Game.Item.Bases.Item(itemType, new Location(101, 100, 7));

        var sourceTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None,
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)), Array.Empty<IItem>(), null);

        var tiles = new ITile[]
        {
            sourceTile,
            new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(101, 100, 7)), Array.Empty<IItem>(), new IItem[] { item }),
            new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(102, 100, 7)), Array.Empty<IItem>(), null)
        };

        var map = MapTestDataBuilder.Build(tiles);

        var sut = MonsterTestDataBuilder.Build(speed: 500, map: map);
        sut.SetNewLocation(new Location(100, 100, 7));

        sut.Metadata.Flags.Add(CreatureFlagAttribute.CanPushItems, 0);

        var gameServer = GameServerTestBuilder.Build(map);
        var cancellationToken = ServerTestHelper.StartThreads(gameServer);
        sut.OnStartedWalking += new CreatureStartedWalkingEventHandler(gameServer).Execute;

        gameServer.Open();

        map.PlaceCreature(sut);

        //act
        sut.WalkTo(new Location(104, 100, 7));

        Task.Delay(1_000, cancellationToken).Wait(cancellationToken);

        //assert
        sut.Tile.Should().Be(sourceTile);
    }
}