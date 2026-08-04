using Moq;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Models.Tiles;
using Serilog;

namespace NeoServer.Domain.Tests.World;

public class TileFactoryTests
{
    [Fact]
    [Trait("Category", "Tile")]
    public void CreateTile_creates_DynamicTile_when_tile_has_door()
    {
        // Arrange — locked door without transformTo would previously become StaticTile
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var door = CreateDoorItem(5098, unpassable: true, transformTo: null);
        var items = new IItem[]
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            door
        };

        // Act
        var tile = tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        // Assert
        tile.Should().BeOfType<DynamicTile>();
        door.IsDoor.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Tile")]
    public void CreateTile_creates_DynamicTile_when_item_has_ActionId()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var keyedItem = ItemTestDataBuilder.CreateUnpassableItem(1);
        keyedItem.Attributes.SetAttribute(ItemAttribute.ActionId, (ushort)1001);

        var items = new IItem[]
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            keyedItem
        };

        // Act
        var tile = tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        // Assert
        tile.Should().BeOfType<DynamicTile>();
    }

    [Fact]
    [Trait("Category", "Tile")]
    public void CreateTile_creates_StaticTile_for_plain_unpassable_item()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var tileFactory = new TileFactory(mockLogger.Object);

        var wall = ItemTestDataBuilder.CreateUnpassableItem(1025);
        var items = new IItem[]
        {
            MapTestDataBuilder.CreateGround(new Location(100, 100, 7)),
            wall
        };

        // Act
        var tile = tileFactory.CreateTile(new Coordinate(100, 100, 7), TileFlag.None, items);

        // Assert
        tile.Should().BeOfType<StaticTile>();
    }

    private static IItem CreateDoorItem(ushort id, bool unpassable, ushort? transformTo)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("door");
        type.Attributes.SetAttribute(ItemTypeAttribute.Type, "door");

        if (unpassable)
        {
            type.SetFlag(ItemFlag.Unpassable);
        }

        if (transformTo is not null)
        {
            type.Attributes.SetAttribute(ItemTypeAttribute.TransformTo, transformTo.Value);
        }

        return new Item(type, new Location(100, 100, 7));
    }
}
