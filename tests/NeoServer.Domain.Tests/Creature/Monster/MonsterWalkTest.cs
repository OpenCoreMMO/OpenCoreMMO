using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Server.Events.Creature;
using xRetry;

namespace NeoServer.Domain.Tests.Creature.Monster;

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

        var item = new Item(itemType, new Location(101, 100, 7));

        var destinationTile = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None,
            MapTestDataBuilder.CreateGround(new Location(102, 100, 7)), [], null);

        var tiles = new ITile[]
        {
            new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(100, 100, 7)), [], null),
            new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(101, 100, 7)), [], [item]),
            destinationTile
        };

        var map = MapTestDataBuilder.Build(tiles);

        var sut = MonsterTestDataBuilder.Build(speed: 6000, map: map);
        sut.SetNewLocation(new Location(100, 100, 7));

        sut.Metadata.Flags.Add(CreatureFlagAttribute.CanPushItems, 1);

        var gameServer = GameServerTestBuilder.Build(map);
        var cancellationToken = ServerTestHelper.StartThreads(gameServer);

        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));

        sut.OnStartedWalking += new CreatureStartedWalkingEventHandler(gameServer, creatureMovementService).Execute;

        gameServer.Open();
        map.PlaceCreature(sut);

        //act
        sut.WalkTo(new Location(103, 100, 7));

        Task.Delay(2_000, cancellationToken).Wait(cancellationToken);

        //assert
        sut.Tile.Should().Be(destinationTile);
    }

    [Fact]
    public async Task Monster_without_can_push_items_flag_do_not_walk()
    {
        //arrange

        // --------------------------------------------------------------------------
        // |  Tile 1 (Monster is here) |  Tile 2 (object is here)  |  Tile 3 (Goal) |
        // --------------------------------------------------------------------------

        var itemType = new ItemType
        {
            Flags = { ItemFlag.BlockPathFind }
        };
        var item = new Item(itemType, new Location(101, 100, 7));

        var sourceTile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None,
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)), [], null);

        var tiles = new ITile[]
        {
            sourceTile,
            new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(101, 100, 7)), [], [item]),
            new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None,
                MapTestDataBuilder.CreateGround(new Location(102, 100, 7)), [], null)
        };

        var map = MapTestDataBuilder.Build(tiles);

        var sut = MonsterTestDataBuilder.Build(speed: 500, map: map);
        sut.SetNewLocation(new Location(100, 100, 7));

        sut.Metadata.Flags.Add(CreatureFlagAttribute.CanPushItems, 0);

        var gameServer = GameServerTestBuilder.Build(map);
        var cancellationToken = ServerTestHelper.StartThreads(gameServer);
        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));

        sut.OnStartedWalking += new CreatureStartedWalkingEventHandler(gameServer, creatureMovementService).Execute;

        gameServer.Open();

        map.PlaceCreature(sut);

        //act
        sut.WalkTo(new Location(104, 100, 7));

        await Task.Delay(1_000, cancellationToken);

        //assert
        sut.Tile.Should().Be(sourceTile);
    }
}