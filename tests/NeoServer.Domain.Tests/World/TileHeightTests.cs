using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Tests.World;

public class TileHeightTests
{
    [Fact]
    public void Tile_that_has_height_is_allowed_to_stack_items_on_it()
    {
        //arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var hasHeightItem = ItemTestDataBuilder.CreateUnpassableItem(1);
        hasHeightItem.Metadata.Flags.Add(ItemFlag.HasHeight);

        var items = new[]
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            hasHeightItem
        };

        var tile = (DynamicTile)tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        var weapon = ItemTestDataBuilder.CreateWeaponItem(1);

        //act
        tile.AddItem(weapon);

        //assert
        tile.ItemsCount.Should().Be(3);
        tile.TopDownItemOnStack.Should().Be(weapon);
    }

    [Fact]
    public void Tile_that_has_depot_is_allowed_to_stack_items_on_it()
    {
        //arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var depot = ItemTestDataBuilder.CreateRegularItem(1);
        depot.Metadata.Flags.Add(ItemFlag.HasHeight);
        depot.Metadata.Attributes.SetAttribute(ItemTypeAttribute.Type, "depot");

        IItem[] items =
        [
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            depot
        ];

        var tile = (DynamicTile)tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        var weapon = ItemTestDataBuilder.CreateWeaponItem(1);

        //act
        tile.AddItem(weapon);

        //assert
        tile.ItemsCount.Should().Be(3);
        tile.TopDownItemOnStack.Should().Be(weapon);
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

        var parcel1 = ItemTestDataBuilder.CreateRegularItem(1);
        var parcel2 = ItemTestDataBuilder.CreateRegularItem(1);
        var parcel3 = ItemTestDataBuilder.CreateRegularItem(1);

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
            var parcel = ItemTestDataBuilder.CreateRegularItem(1);
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

        var parcel1 = ItemTestDataBuilder.CreateRegularItem(1);
        var parcel2 = ItemTestDataBuilder.CreateRegularItem(1);
        var parcel3 = ItemTestDataBuilder.CreateRegularItem(1);

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
            var parcel = ItemTestDataBuilder.CreateRegularItem(1);
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