using FluentAssertions;
using Moq;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.World.Factories;
using NeoServer.Game.World.Models.Tiles;
using Serilog;
using Xunit;

namespace NeoServer.Game.World.Tests;

public class TileHeightTests
{
    [Fact]
    public void Tile_that_has_height_is_allowed_to_stack_items_on_it()
    {
        //arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var hasHeightItem = ItemTestData.CreateUnpassableItem(1);
        hasHeightItem.Metadata.Flags.Add(ItemFlag.HasHeight);

        var items = new[]
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            hasHeightItem
        };

        var tile = (DynamicTile)tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        var weapon = ItemTestData.CreateWeaponItem(1);

        //act
        tile.AddItem(weapon);

        //assert
        tile.ItemsCount.Should().Be(3);
        tile.TopItemOnStack.Should().Be(weapon);
    }

    [Fact]
    public void Tile_that_has_depot_is_allowed_to_stack_items_on_it()
    {
        //arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var depot = ItemTestData.CreateRegularItem(1);
        depot.Metadata.Flags.Add(ItemFlag.HasHeight);
        depot.Metadata.Attributes.SetAttribute(ItemAttribute.Type, "depot");

        IItem[] items =
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            depot
        };

        var tile = (DynamicTile)tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        var weapon = ItemTestData.CreateWeaponItem(1);

        //act
        tile.AddItem(weapon);

        //assert
        tile.ItemsCount.Should().Be(3);
        tile.TopItemOnStack.Should().Be(weapon);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Player_can_move_to_above_floor_using_3_parcels(bool withMore3ParcelsAtDestinationOrOriginTile)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var tile1StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2StFloor = MapTestDataBuilder.CreateTile(new Location(101, 100, 6));

        var parcel1 = ItemTestData.CreateRegularItem(1);
        var parcel2 = ItemTestData.CreateRegularItem(1);
        var parcel3 = ItemTestData.CreateRegularItem(1);

        parcel1.Metadata.Flags.Add(ItemFlag.HasHeight);
        parcel2.Metadata.Flags.Add(ItemFlag.HasHeight);
        parcel3.Metadata.Flags.Add(ItemFlag.HasHeight);

        var map = MapTestDataBuilder.Build(tile1StFloor, tile2StFloor);

        tile1StFloor.AddItem(parcel1);
        tile1StFloor.AddItem(parcel2);
        tile1StFloor.AddItem(parcel3);

        if (withMore3ParcelsAtDestinationOrOriginTile)
        {
            tile2StFloor.AddItem(parcel1);
            tile2StFloor.AddItem(parcel2);
            tile2StFloor.AddItem(parcel3);
        }

        tile1StFloor.AddCreature(player);

        //act
        player.WalkTo(Direction.East);
        map.MoveCreature(player);

        //assert
        player.Tile.Should().Be(tile2StFloor);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(0)]
    public void Player_cant_move_to_above_floor_using_less_than_3_parcels(int numberOfParcels)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var tile1StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2StFloor = MapTestDataBuilder.CreateTile(new Location(101, 100, 6));

        for (var i = 0; i < numberOfParcels; i++)
        {
            var parcel = ItemTestData.CreateRegularItem(1);
            parcel.Metadata.Flags.Add(ItemFlag.HasHeight);
            tile1StFloor.AddItem(parcel);
        }

        var map = MapTestDataBuilder.Build(tile1StFloor, tile2StFloor);

        tile1StFloor.AddCreature(player);

        //act
        player.WalkTo(Direction.East);
        map.MoveCreature(player);

        //assert
        player.Tile.Should().Be(tile1StFloor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [ThreadBlocking]
    public void Player_can_move_to_below_floor_using_3_parcels(bool withMore3ParcelsAtDestinationOrOriginTile)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var tile1StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 6));

        var parcel1 = ItemTestData.CreateRegularItem(1);
        var parcel2 = ItemTestData.CreateRegularItem(1);
        var parcel3 = ItemTestData.CreateRegularItem(1);

        parcel1.Metadata.Flags.Add(ItemFlag.HasHeight);
        parcel2.Metadata.Flags.Add(ItemFlag.HasHeight);
        parcel3.Metadata.Flags.Add(ItemFlag.HasHeight);

        var map = MapTestDataBuilder.Build(tile1StFloor, tile2StFloor);

        tile1StFloor.AddItem(parcel1);
        tile1StFloor.AddItem(parcel2);
        tile1StFloor.AddItem(parcel3);

        if (withMore3ParcelsAtDestinationOrOriginTile)
        {
            tile2StFloor.AddItem(parcel1);
            tile2StFloor.AddItem(parcel2);
            tile2StFloor.AddItem(parcel3);
        }

        tile2StFloor.AddCreature(player);

        //act
        player.WalkTo(Direction.West);
        map.MoveCreature(player);

        //assert
        player.Tile.Should().Be(tile1StFloor);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(0)]
    [ThreadBlocking]
    public void Player_cant_move_to_below_floor_using_less_than_3_parcels(int numberOfParcels)
    {
        //arrange
        var player = PlayerTestDataBuilder.Build();
        var tile1StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(100, 100, 7));
        var tile2StFloor = (DynamicTile)MapTestDataBuilder.CreateTile(new Location(101, 100, 6));

        for (var i = 0; i < numberOfParcels; i++)
        {
            var parcel = ItemTestData.CreateRegularItem(1);
            parcel.Metadata.Flags.Add(ItemFlag.HasHeight);
            tile1StFloor.AddItem(parcel);
        }

        var map = MapTestDataBuilder.Build(tile1StFloor, tile2StFloor);

        tile2StFloor.AddCreature(player);

        //act
        player.WalkTo(Direction.West);
        map.MoveCreature(player);

        //assert
        player.Tile.Should().Be(tile2StFloor);
    }
}