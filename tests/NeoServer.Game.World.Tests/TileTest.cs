using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Services;
using NeoServer.Game.Items;
using NeoServer.Game.Items.Items;
using NeoServer.Game.Tests.Helpers;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Helpers.Player;
using NeoServer.Game.Tests.Server;
using NeoServer.Game.World.Models.Tiles;
using NeoServer.Game.World.Services;
using NeoServer.Server.Commands.Movements;
using Xunit;

namespace NeoServer.Game.World.Tests;

public class RemoveThingTileTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 100), 40, 500, 60 };
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 50), 49, 500, 1 };
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 50), 1, 500, 49 };
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 1), 1, 400, 32 };
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 100), 100, 400, 32 };
        yield return new object[] { ItemTestData.CreateCumulativeItem(500, 45), 45, 400, 32 };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class TileTest
{
    public static IEnumerable<object[]> NextTilesTestData =>
        new List<object[]>
        {
            new object[]
            {
                new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(101, 101, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(100, 99, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 99, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(101, 99, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 101, 7), TileFlag.None, null, Array.Empty<IItem>(),
                    Array.Empty<IItem>())
            }
        };

    private DynamicTile CreateTile(params IItem[] item)
    {
        var topItems = new List<IItem>
        {
            ItemTestData.CreateTopItem(1, 1)
        };
        var items = new List<IItem>
        {
            ItemTestData.CreateRegularItem(100),
            ItemTestData.CreateRegularItem(200)
        };
        items.AddRange(item);

        var tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, topItems.ToArray(),
            items.ToArray());
        return tile;
    }

    [Fact]
    public void Constructor_Given_Items_Creates_Tile()
    {
        var tile = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, new List<IItem>
        {
            ItemTestData.CreateTopItem(2, 1)
        }.ToArray(), new List<IItem>
        {
            ItemTestData.CreateRegularItem(5),
            ItemTestData.CreateRegularItem(6),
            ItemTestData.CreateRegularItem(6),
            ItemTestData.CreateCumulativeItem(7, 35),
            ItemTestData.CreateCumulativeItem(7, 80),
            ItemTestData.CreateCumulativeItem(8, 3)
        }.ToArray());

        var downItemsExpected = new List<IItem>
        {
            ItemTestData.CreateRegularItem(5),
            ItemTestData.CreateRegularItem(6),
            ItemTestData.CreateRegularItem(6),
            ItemTestData.CreateCumulativeItem(7, 35),
            ItemTestData.CreateCumulativeItem(7, 80),
            ItemTestData.CreateCumulativeItem(8, 3)
        };

        var top1Expected = new List<IItem>
        {
            ItemTestData.CreateTopItem(2, 1)
        };

        Assert.Collection(tile.TopItems, item => Assert.Equal(top1Expected[0].ClientId, item.ClientId));

        Assert.Collection(tile.DownItems, item =>
            {
                Assert.Equal(downItemsExpected[5].ClientId, item.ClientId);
                Assert.Equal((downItemsExpected[5] as ICumulative).Amount, (item as ICumulative).Amount);
            },
            item =>
            {
                Assert.Equal(downItemsExpected[4].ClientId, item.ClientId);
                Assert.Equal((downItemsExpected[4] as ICumulative).Amount, (item as ICumulative).Amount);
            },
            item =>
            {
                Assert.Equal(downItemsExpected[3].ClientId, item.ClientId);
                Assert.Equal((downItemsExpected[3] as ICumulative).Amount, (item as ICumulative).Amount);
            },
            item => Assert.Equal(downItemsExpected[2].ClientId, item.ClientId),
            item => Assert.Equal(downItemsExpected[1].ClientId, item.ClientId),
            item => Assert.Equal(downItemsExpected[0].ClientId, item.ClientId));
    }

    [Fact]
    public void RemoveThing_Removes_Item_From_Stack()
    {
        var item = ItemTestData.CreateMoveableItem(500);
        var sut = CreateTile(item);

        sut.RemoveItem(item, 1, 0, out var removedThing);

        Assert.Equal(2, sut.DownItems.Count);
        Assert.Single(sut.TopItems);

        Assert.Equal(200, sut.DownItems.First().ClientId);
    }

    [Theory]
    [ClassData(typeof(RemoveThingTileTestData))]
    public void RemoveThing_Removes_CumulativeItem_From_Stack(ICumulative item, byte amountToRemove,
        ushort topItemId, byte remainingAmount)
    {
        var item2 = ItemTestData.CreateCumulativeItem(400, 32);
        var sut = CreateTile(item2, item);

        sut.RemoveItem(item, amountToRemove, 0, out var removedThing);

        Assert.Equal(topItemId, sut.DownItems.First().ClientId);
        Assert.Equal(remainingAmount, (sut.DownItems.First() as ICumulative).Amount);
    }

    [Fact]
    public void AddThing_When_Cumulative_On_Top_Join_If_Same_Type()
    {
        var item = ItemTestData.CreateThrowableDistanceItem(500, 5);
        var sut = CreateTile(item);

        var item2 = ItemTestData.CreateThrowableDistanceItem(500, 3);
        sut.AddItem(item2);

        Assert.Equal(3, sut.DownItems.Count);
        Assert.Single(sut.TopItems);

        Assert.Equal(500, sut.DownItems.First().ClientId);
        Assert.Equal(8, (sut.DownItems.First() as ICumulative).Amount);
    }

    [Fact]
    public void AddThing_When_Cumulative_On_Top_Join_If_Same_Type_And_Creates_New_Item_When_Overflows()
    {
        var item = ItemTestData.CreateThrowableDistanceItem(500, 60);
        var sut = CreateTile(item);

        var item2 = ItemTestData.CreateThrowableDistanceItem(500, 100);
        sut.AddItem(item2);

        Assert.Equal(4, sut.DownItems.Count);
        Assert.Single(sut.TopItems);

        Assert.Equal(500, sut.DownItems.First().ClientId);
        Assert.Equal(60, (sut.DownItems.First() as ICumulative).Amount);

        Assert.Equal(500, sut.DownItems.Skip(1).Take(1).First().ClientId);
        Assert.Equal(100, (sut.DownItems.Skip(1).Take(1).First() as ICumulative).Amount);
    }

    [Theory]
    [MemberData(nameof(NextTilesTestData))]
    public void IsNextTo_When_1_Sqm_Distant_Returns_True(ITile dest)
    {
        ITile sut = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());

        Assert.True(sut.IsNextTo(dest));
    }

    [Fact]
    public void IsNextTo_When_2_Or_More_Sqm_Distant_Returns_True()
    {
        ITile sut = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());
        ITile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, Array.Empty<IItem>(),
            Array.Empty<IItem>());

        Assert.False(sut.IsNextTo(dest));
    }

    [Fact]
    public void Item_falls_when_moved_to_a_hole()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var mapService = new MapService(map);

        var item = ItemTestData.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];

        mapService.ReplaceGround(destinationTile.Location, hole);

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler));

        sourceTile.AddItem(item);

        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService);

        //act
        toMapMovementService.Move(player,
            new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopItemOnStack.Should().NotBe(item);
        destinationTile.TopItemOnStack.Should().NotBe(item);
        undergroundTile.TopItemOnStack.Should().Be(item);
    }

    [Fact]
    [ThreadBlocking]
    public void Item_doesnt_go_to_hole_if_the_final_tile_is_blocked()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8,
            staticTiles: new List<Location>
            {
                new(100, 100, 8)
            });

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var item = ItemTestData.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = map[100, 100, 8];

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler));

        var mapService = new MapService(map);

        mapService.ReplaceGround(destinationTile.Location, hole);

        sourceTile.AddItem(item);

        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService);

        //act
        toMapMovementService.Move(player, new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopItemOnStack.Should().Be(item);
        destinationTile.TopItemOnStack.Should().NotBe(item);
        undergroundTile.TopItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Item_falls_two_floors_if_a_hole_is_below_another_hole()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 9);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var mapService = new MapService(map);

        var item = ItemTestData.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var secondHole = new Ground(new ItemType(), new Location(100, 100, 8));
        secondHole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];
        var secondFloor = (IDynamicTile)map[100, 100, 9];

        sourceTile.AddItem(item);

        mapService.ReplaceGround(destinationTile.Location, hole);

        mapService.ReplaceGround(undergroundTile.Location, secondHole);

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler));
        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService);

        //act
        toMapMovementService.Move(player, new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopItemOnStack.Should().NotBe(item);
        destinationTile.TopItemOnStack.Should().NotBe(item);
        undergroundTile.TopItemOnStack.Should().NotBe(item);
        secondFloor.TopItemOnStack.Should().Be(item);
    }

    [Fact]
    [ThreadBlocking]
    public void Items_fall_when_a_hole_is_opened_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        var mapService = new MapService(map);

        player.SetNewLocation(new Location(102, 100, 7));

        var item = ItemTestData.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];

        sourceTile.AddItem(item);

        player.MoveItem(item, sourceTile, destinationTile, 1, 0, 0);

        //act
        mapService.ReplaceGround(destinationTile.Location, hole);

        //assert
        sourceTile.TopItemOnStack.Should().NotBe(item);
        destinationTile.TopItemOnStack.Should().NotBe(item);
        undergroundTile.TopItemOnStack.Should().Be(item);
    }

    [Fact]
    public void Creature_falls_when_a_hole_is_opened_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var mapService = new MapService(map);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(100, 100, 7));

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemAttribute.FloorChange, "down");

        var tile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];

        map.PlaceCreature(player);

        //act
        mapService.ReplaceGround(tile.Location, hole);

        //assert
        tile.TopCreatureOnStack.Should().NotBe(player);
        undergroundTile.TopCreatureOnStack.Should().Be(player);
    }

    [Fact]
    [ThreadBlocking]
    public void Player_cannot_move_item_to_unpassable_tile()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();

        var unpassableItem = ItemTestData.CreateUnpassableItem(1);

        var itemToMove = ItemTestData.CreateWeaponItem(2);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];

        sourceTile.AddItem(itemToMove);
        destinationTile.AddItem(unpassableItem);

        //act
        player.MoveItem(itemToMove, sourceTile, destinationTile, 1, 0, 0);

        //assert
        sourceTile.TopItemOnStack.Should().Be(itemToMove);
        destinationTile.TopItemOnStack.Should().Be(unpassableItem);
    }
}