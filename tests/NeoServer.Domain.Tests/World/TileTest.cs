using System.Collections;
using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Locker;
using NeoServer.Domain.Mail;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;
using NeoServer.Server.Commands.Movements;

namespace NeoServer.Domain.Tests.World;

public class RemoveThingTileTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 100), 40, 500, 60];
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 50), 49, 500, 1];
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 50), 1, 500, 49];
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 1), 1, 400, 32];
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 100), 100, 400, 32];
        yield return [ItemTestDataBuilder.CreateCumulativeItem(500, 45), 45, 400, 32];
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
                new DynamicTile(new Coordinate(101, 100, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(101, 101, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(100, 101, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 100, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(100, 99, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 99, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(101, 99, 7), TileFlag.None, null, [],
                    [])
            },
            new object[]
            {
                new DynamicTile(new Coordinate(99, 101, 7), TileFlag.None, null, [],
                    [])
            }
        };

    private DynamicTile CreateTile(params IItem[] item)
    {
        var topItems = new List<IItem>
        {
            ItemTestDataBuilder.CreateTopItem(1, 1)
        };
        var items = new List<IItem>
        {
            ItemTestDataBuilder.CreateRegularItem(100),
            ItemTestDataBuilder.CreateRegularItem(200)
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
            ItemTestDataBuilder.CreateTopItem(2, 1)
        }.ToArray(), new List<IItem>
        {
            ItemTestDataBuilder.CreateRegularItem(5),
            ItemTestDataBuilder.CreateRegularItem(6),
            ItemTestDataBuilder.CreateRegularItem(6),
            ItemTestDataBuilder.CreateCumulativeItem(7, 35),
            ItemTestDataBuilder.CreateCumulativeItem(7, 80),
            ItemTestDataBuilder.CreateCumulativeItem(8, 3)
        }.ToArray());

        var downItemsExpected = new List<IItem>
        {
            ItemTestDataBuilder.CreateRegularItem(5),
            ItemTestDataBuilder.CreateRegularItem(6),
            ItemTestDataBuilder.CreateRegularItem(6),
            ItemTestDataBuilder.CreateCumulativeItem(7, 35),
            ItemTestDataBuilder.CreateCumulativeItem(7, 80),
            ItemTestDataBuilder.CreateCumulativeItem(8, 3)
        };

        var top1Expected = new List<IItem>
        {
            ItemTestDataBuilder.CreateTopItem(2, 1)
        };

        var item = Assert.Single(tile.TopItems);
        Assert.Equal(top1Expected[0].ClientId, item.ClientId);

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
        var item = ItemTestDataBuilder.CreateMoveableItem(500);
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
        var item2 = ItemTestDataBuilder.CreateCumulativeItem(400, 32);
        var sut = CreateTile(item2, item);

        sut.RemoveItem(item, amountToRemove, 0, out var removedThing);

        Assert.Equal(topItemId, sut.DownItems.First().ClientId);
        Assert.Equal(remainingAmount, (sut.DownItems.First() as ICumulative).Amount);
    }

    [Fact]
    public void AddThing_When_Cumulative_On_Top_Join_If_Same_Type()
    {
        var item = ItemTestDataBuilder.CreateThrowableDistanceItem(500, 5);
        var sut = CreateTile(item);

        var item2 = ItemTestDataBuilder.CreateThrowableDistanceItem(500, 3);
        sut.AddItem(item2);

        Assert.Equal(3, sut.DownItems.Count);
        Assert.Single(sut.TopItems);

        Assert.Equal(500, sut.DownItems.First().ClientId);
        Assert.Equal(8, (sut.DownItems.First() as ICumulative).Amount);
    }

    [Fact]
    public void AddThing_When_Cumulative_On_Top_Join_If_Same_Type_And_Creates_New_Item_When_Overflows()
    {
        var item = ItemTestDataBuilder.CreateThrowableDistanceItem(500, 60);
        var sut = CreateTile(item);

        var item2 = ItemTestDataBuilder.CreateThrowableDistanceItem(500, 100);
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
        ITile sut = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, [],
            []);

        Assert.True(sut.IsNextTo(dest));
    }

    [Fact]
    public void IsNextTo_When_2_Or_More_Sqm_Distant_Returns_True()
    {
        ITile sut = new DynamicTile(new Coordinate(100, 100, 7), TileFlag.None, null, [],
            []);
        ITile dest = new DynamicTile(new Coordinate(102, 100, 7), TileFlag.None, null, [],
            []);

        Assert.False(sut.IsNextTo(dest));
    }

    [Fact]
    public void Item_falls_when_moved_to_a_hole()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));
        var mapService = new MapService(map, creatureMovementService);

        var item = ItemTestDataBuilder.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];

        mapService.ReplaceGround(destinationTile.Location, hole);
        
        var mailService = new MailService(new Mock<IPlayerRepository>().Object, new Mock<IPlayerMailRepository>().Object, new LockerManager(), null);

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler), mailService);

        sourceTile.AddItem(item);

        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService, new Mock<ICreaturePushService>().Object);

        //act
        toMapMovementService.Move(player,
            new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopDownItemOnStack.Should().NotBe(item);
        destinationTile.TopDownItemOnStack.Should().NotBe(item);
        undergroundTile.TopDownItemOnStack.Should().Be(item);
    }

    [Fact]
    [ThreadBlocking]
    public void Item_doesnt_go_to_hole_if_the_final_tile_is_blocked()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8,
            staticTiles: [new(100, 100, 8)]);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var item = ItemTestDataBuilder.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = map[100, 100, 8];

        var mailService = new MailService(new Mock<IPlayerRepository>().Object, new Mock<IPlayerMailRepository>().Object, new LockerManager(), null);

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler), mailService);

        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));
        var mapService = new MapService(map, creatureMovementService);

        mapService.ReplaceGround(destinationTile.Location, hole);

        sourceTile.AddItem(item);

        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService, new Mock<ICreaturePushService>().Object);

        //act
        toMapMovementService.Move(player, new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopDownItemOnStack.Should().Be(item);
        destinationTile.TopDownItemOnStack.Should().NotBe(item);
        undergroundTile.TopDownItemOnStack.Should().NotBe(item);
    }

    [Fact]
    public void Item_falls_two_floors_if_a_hole_is_below_another_hole()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 9);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(102, 100, 7));

        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));
        var mapService = new MapService(map, creatureMovementService);

        var item = ItemTestDataBuilder.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var secondHole = new Ground(new ItemType(), new Location(100, 100, 8));
        secondHole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];
        var secondFloor = (IDynamicTile)map[100, 100, 9];

        sourceTile.AddItem(item);

        mapService.ReplaceGround(destinationTile.Location, hole);

        mapService.ReplaceGround(undergroundTile.Location, secondHole);
        
        var mailService = new MailService(new Mock<IPlayerRepository>().Object, new Mock<IPlayerMailRepository>().Object, new LockerManager(), null);

        var itemMovementService =
            new ItemMovementService(new WalkToMechanism(GameServerTestBuilder.Build(map).Scheduler), mailService);
        var toMapMovementService = new ToMapMovementService(map, mapService, itemMovementService, new Mock<ICreaturePushService>().Object);

        //act
        toMapMovementService.Move(player, new MovementParams(sourceTile.Location, destinationTile.Location, 1));

        //assert
        sourceTile.TopDownItemOnStack.Should().NotBe(item);
        destinationTile.TopDownItemOnStack.Should().NotBe(item);
        undergroundTile.TopDownItemOnStack.Should().NotBe(item);
        secondFloor.TopDownItemOnStack.Should().Be(item);
    }

    [Fact]
    [ThreadBlocking]
    public void Items_fall_when_a_hole_is_opened_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        var player = PlayerTestDataBuilder.Build();
        
        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));
        var mapService = new MapService(map, creatureMovementService);

        player.SetNewLocation(new Location(102, 100, 7));

        var item = ItemTestDataBuilder.CreateWeaponItem(1);

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

        map.PlaceCreature(player);

        var sourceTile = (IDynamicTile)map[101, 100, 7];
        var destinationTile = (IDynamicTile)map[100, 100, 7];
        var undergroundTile = (IDynamicTile)map[100, 100, 8];

        sourceTile.AddItem(item);

        player.MoveItem(item, sourceTile, destinationTile, 1, 0, 0);

        //act
        mapService.ReplaceGround(destinationTile.Location, hole);

        //assert
        sourceTile.TopDownItemOnStack.Should().NotBe(item);
        destinationTile.TopDownItemOnStack.Should().NotBe(item);
        undergroundTile.TopDownItemOnStack.Should().Be(item);
    }

    [Fact]
    public void Creature_falls_when_a_hole_is_opened_in_the_ground()
    {
        //arrange
        var map = MapTestDataBuilder.Build(100, 105, 100, 105, 7, 8);
        
        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));
        var mapService = new MapService(map, creatureMovementService);

        var player = PlayerTestDataBuilder.Build();
        player.SetNewLocation(new Location(100, 100, 7));

        var hole = new Ground(new ItemType(), new Location(100, 100, 7));
        hole.Metadata.Attributes.SetAttribute(ItemTypeAttribute.FloorChange, "down");

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
    public void GetItemByIndex_Should_Return_Correct_Item_At_Index_Without_Ground()
    {
        // Arrange
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);
        var downItem = ItemTestDataBuilder.CreateRegularItem(2);

        var tile = new DynamicTile(
            new Coordinate(100, 100, 7),
            TileFlag.None,
            null, // No ground
            [topItem],
            [downItem]
        );

        // Act & Assert
        // Index 0 should return first top item (no ground)
        tile.GetItemByIndex(0).Should().Be(topItem);

        // Index 1 should return first down item
        tile.GetItemByIndex(1).Should().Be(downItem);

        // Index 2 should return null (out of range)
        tile.GetItemByIndex(2).Should().BeNull();
    }

    [Fact]
    public void StaticTile_GetItemByIndex_Should_Return_Correct_Item_At_Index_Without_Ground()
    {
        // Arrange
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);
        var downItem = ItemTestDataBuilder.CreateRegularItem(2);

        var tile = new StaticTile(new Coordinate(100, 100, 7), topItem, downItem);

        // Act & Assert
        // Index 0 should return first top item (no ground)
        tile.GetItemByIndex(0).Should().Be(topItem);

        // Index 1 should return first down item
        tile.GetItemByIndex(1).Should().Be(downItem);

        // Index 2 should return null (out of range)
        tile.GetItemByIndex(2).Should().BeNull();
    }

    [Fact]
    public void StaticTile_GetItemByIndex_Should_Return_Correct_Item_At_Index_With_Ground()
    {
        // Arrange
        var ground = MapTestDataBuilder.CreateGround(new Location(100, 100, 7), 100);
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);
        var downItem = ItemTestDataBuilder.CreateRegularItem(2);

        var tile = new StaticTile(new Coordinate(100, 100, 7), ground, topItem, downItem);

        // Act & Assert
        // Index 0 should return ground
        tile.GetItemByIndex(0).Should().Be(ground);

        // Index 1 should return top item
        tile.GetItemByIndex(1).Should().Be(topItem);

        // Index 2 should return down item
        tile.GetItemByIndex(2).Should().Be(downItem);

        // Index 3 should return null (out of range)
        tile.GetItemByIndex(3).Should().BeNull();
    }

    [Fact]
    public void StaticTile_GetItemByIndex_Should_Return_Null_For_Negative_Index()
    {
        // Arrange
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);

        var tile = new StaticTile(new Coordinate(100, 100, 7), topItem);

        // Act & Assert
        tile.GetItemByIndex(-1).Should().BeNull();
    }

    [Fact]
    public void StaticTile_GetItemByIndex_Should_Return_Null_For_Out_Of_Range_Index()
    {
        // Arrange
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);

        var tile = new StaticTile(new Coordinate(100, 100, 7), topItem);

        // Act & Assert
        tile.GetItemByIndex(1).Should().BeNull();
    }

    [Fact]
    public void StaticTile_GetItemByIndex_Should_Work_Correctly_With_Items_In_Random_Order()
    {
        // Arrange
        var topItem = ItemTestDataBuilder.CreateTopItem(1, 1);
        var ground = MapTestDataBuilder.CreateGround(new Location(100, 100, 7), 100);
        var downItem = ItemTestDataBuilder.CreateRegularItem(2);

        // Pass items in random order: topItem, ground, downItem
        var tile = new StaticTile(new Coordinate(100, 100, 7), topItem, ground, downItem);

        // Act & Assert
        // Even though passed in random order, AllItems should be ordered: ground, topItem, downItem
        // Index 0 should return ground
        tile.GetItemByIndex(0).Should().Be(ground);

        // Index 1 should return top item
        tile.GetItemByIndex(1).Should().Be(topItem);

        // Index 2 should return down item
        tile.GetItemByIndex(2).Should().Be(downItem);

        // Index 3 should return null (out of range)
        tile.GetItemByIndex(3).Should().BeNull();
    }
}