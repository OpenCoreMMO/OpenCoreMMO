using Moq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Services;

/// <summary>
///     Unit tests for StaticToDynamicTileService.
///     Tests cover: transforming static tiles to dynamic tiles, handling items, edge cases with invalid tiles.
/// </summary>
public class StaticToDynamicTileServiceTests
{
    #region Helper Methods

    private static StaticToDynamicTileService CreateService(
        IItemClientServerIdMapStore itemClientServerIdMapStore = null,
        IItemFactory itemFactory = null,
        ITileFactory tileFactory = null,
        Domain.World.World world = null)
    {
        itemClientServerIdMapStore ??= CreateItemClientServerIdMapStore();
        itemFactory ??= CreateItemFactory();
        tileFactory ??= CreateTileFactory();
        world ??= CreateWorld();

        return new StaticToDynamicTileService(
            itemClientServerIdMapStore,
            itemFactory,
            tileFactory,
            world);
    }

    private static IItemClientServerIdMapStore CreateItemClientServerIdMapStore(
        params (ushort clientId, ushort serverId)[] mappings)
    {
        var mock = new Mock<IItemClientServerIdMapStore>();

        foreach (var (clientId, serverId) in mappings)
            mock.Setup(x => x.TryGetValue(clientId, out It.Ref<ushort>.IsAny))
                .Returns((ushort key, out ushort value) =>
                {
                    value = serverId;
                    return true;
                });

        return mock.Object;
    }

    private static IItemFactory CreateItemFactory()
    {
        var mock = new Mock<IItemFactory>();

        mock.Setup(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IDictionary<ItemAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IEnumerable<IItem>>()))
            .Returns((ushort serverId, Location location,
                IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
                IDictionary<string, IConvertible> itemTypeCustomAttributes,
                IDictionary<ItemAttribute, IConvertible> itemAttributes,
                IDictionary<string, IConvertible> itemCustomAttributes, IEnumerable<IItem> children) =>
            {
                return ItemTestDataBuilder.CreateRegularItem(serverId);
            });

        return mock.Object;
    }

    private static ITileFactory CreateTileFactory()
    {
        var mock = new Mock<ITileFactory>();

        mock.Setup(x => x.CreateDynamicTile(
                It.IsAny<Coordinate>(),
                It.IsAny<TileFlag>(),
                It.IsAny<IItem[]>()))
            .Returns((Coordinate coordinate, TileFlag flag, IItem[] items) =>
            {
                return new DynamicTile(coordinate, flag, null, [], items);
            });

        return mock.Object;
    }

    private static Domain.World.World CreateWorld()
    {
        return new Domain.World.World();
    }

    private static StaticTile CreateStaticTile(Location location, params IItem[] items)
    {
        return new StaticTile(location, items);
    }

    private static DynamicTile CreateDynamicTile(Location location)
    {
        return new DynamicTile(new Coordinate(location.X, location.Y, (sbyte)location.Z), TileFlag.None, null, [], []);
    }

    private static Location CreateLocation(ushort x = 100, ushort y = 100, byte z = 7)
    {
        return new Location(x, y, z);
    }

    #endregion

    #region TransformIntoDynamicTile Tests

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Service_transforms_static_tile_into_dynamic_tile()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location);
        world.AddTile(staticTile, location);

        var itemClientServerIdMapStore = CreateItemClientServerIdMapStore();
        var service = CreateService(
            itemClientServerIdMapStore,
            world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IDynamicTile>();
        result.Should().NotBe(staticTile);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Service_replaces_tile_in_world_when_transforming()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location);
        world.AddTile(staticTile, location);

        var service = CreateService(world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        world.TryGetTile(ref location, out var tileInWorld).Should().BeTrue();
        tileInWorld.Should().Be(result);
        tileInWorld.Should().BeAssignableTo<IDynamicTile>();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Service_creates_dynamic_tile_with_same_location()
    {
        // Arrange
        var location = CreateLocation(105, 205, 8);
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location);
        world.AddTile(staticTile, location);

        var service = CreateService(world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        result.Location.X.Should().Be(105);
        result.Location.Y.Should().Be(205);
        result.Location.Z.Should().Be(8);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Service_transforms_items_from_static_to_dynamic_tile()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var item1 = ItemTestDataBuilder.CreateRegularItem(100);
        var item2 = ItemTestDataBuilder.CreateRegularItem(200);
        var staticTile = CreateStaticTile(location, item1, item2);
        world.AddTile(staticTile, location);

        // Setup client to server ID mappings
        var itemClientServerIdMapStoreMock = new Mock<IItemClientServerIdMapStore>();
        itemClientServerIdMapStoreMock.Setup(x => x.TryGetValue(It.IsAny<ushort>(), out It.Ref<ushort>.IsAny))
            .Returns((ushort key, out ushort value) =>
            {
                value = (ushort)(key + 1000); // Map client ID to a different server ID
                return true;
            });

        var itemFactoryMock = new Mock<IItemFactory>();
        var createdItems = new List<IItem>();
        itemFactoryMock.Setup(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IDictionary<ItemAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IEnumerable<IItem>>()))
            .Returns((ushort serverId, Location loc, IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
                IDictionary<string, IConvertible> itemTypeCustomAttributes,
                IDictionary<ItemAttribute, IConvertible> itemAttributes,
                IDictionary<string, IConvertible> itemCustomAttributes, IEnumerable<IItem> children) =>
            {
                var item = ItemTestDataBuilder.CreateRegularItem(serverId);
                createdItems.Add(item);
                return item;
            });

        var tileFactoryMock = new Mock<ITileFactory>();
        tileFactoryMock.Setup(x => x.CreateDynamicTile(
                It.IsAny<Coordinate>(),
                It.IsAny<TileFlag>(),
                It.IsAny<IItem[]>()))
            .Returns((Coordinate coordinate, TileFlag flag, IItem[] items) =>
            {
                return new DynamicTile(coordinate, flag, null, [], items);
            });

        var service = CreateService(
            itemClientServerIdMapStoreMock.Object,
            itemFactoryMock.Object,
            tileFactoryMock.Object,
            world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        itemFactoryMock.Verify(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()),
            Times.Exactly(staticTile.AllClientIdItems.Length));

        createdItems.Should().HaveCount(staticTile.AllClientIdItems.Length);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Service_returns_original_tile_when_tile_has_no_location()
    {
        // Arrange
        var emptyLocation = new Location(0, 0, 0);
        var staticTile = CreateStaticTile(emptyLocation);
        var service = CreateService();

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        result.Should().Be(staticTile);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Service_returns_original_tile_when_tile_is_not_static()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var dynamicTile = CreateDynamicTile(location);
        world.AddTile(dynamicTile, location);

        var service = CreateService(world: world);

        // Act
        var result = service.TransformIntoDynamicTile(dynamicTile);

        // Assert
        result.Should().Be(dynamicTile);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Service_clones_all_items_using_server_id_when_static_tile_has_item_instances()
    {
        // Arrange — CloneItems uses original.ServerId; client-id map is not consulted.
        var location = CreateLocation();
        var world = CreateWorld();
        var item1 = ItemTestDataBuilder.CreateRegularItem(100);
        var item2 = ItemTestDataBuilder.CreateRegularItem(200);
        var item3 = ItemTestDataBuilder.CreateRegularItem(300);
        var staticTile = CreateStaticTile(location, item1, item2, item3);
        world.AddTile(staticTile, location);

        var itemFactoryMock = new Mock<IItemFactory>();
        var createdItems = new List<IItem>();
        itemFactoryMock.Setup(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IDictionary<ItemAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IEnumerable<IItem>>()))
            .Returns((ushort serverId, Location loc, IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
                IDictionary<string, IConvertible> itemTypeCustomAttributes,
                IDictionary<ItemAttribute, IConvertible> itemAttributes,
                IDictionary<string, IConvertible> itemCustomAttributes, IEnumerable<IItem> children) =>
            {
                var item = ItemTestDataBuilder.CreateRegularItem(serverId);
                createdItems.Add(item);
                return item;
            });

        var service = CreateService(
            itemFactory: itemFactoryMock.Object,
            world: world);

        // Act
        service.TransformIntoDynamicTile(staticTile);

        // Assert
        itemFactoryMock.Verify(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()),
            Times.Exactly(3));

        createdItems.Should().HaveCount(3);
        itemFactoryMock.Verify(x => x.Create(100, It.IsAny<Location>(),
            It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()), Times.Once);
        itemFactoryMock.Verify(x => x.Create(200, It.IsAny<Location>(),
            It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()), Times.Once);
        itemFactoryMock.Verify(x => x.Create(300, It.IsAny<Location>(),
            It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Service_creates_empty_dynamic_tile_when_static_tile_has_no_items()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location); // No items
        world.AddTile(staticTile, location);

        var itemFactoryMock = new Mock<IItemFactory>();
        var tileFactoryMock = new Mock<ITileFactory>();
        IItem[] capturedItems = null;

        tileFactoryMock.Setup(x => x.CreateDynamicTile(
                It.IsAny<Coordinate>(),
                It.IsAny<TileFlag>(),
                It.IsAny<IItem[]>()))
            .Returns((Coordinate coordinate, TileFlag flag, IItem[] items) =>
            {
                capturedItems = items;
                return new DynamicTile(coordinate, flag, null, [], items);
            });

        var service = CreateService(
            itemFactory: itemFactoryMock.Object,
            tileFactory: tileFactoryMock.Object,
            world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IDynamicTile>();
        itemFactoryMock.Verify(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()),
            Times.Never());

        capturedItems.Should().NotBeNull();
        capturedItems.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Service_calls_item_factory_with_correct_parameters()
    {
        // Arrange — clone path uses original.ServerId and tile location.
        var location = CreateLocation(150, 250, 5);
        var world = CreateWorld();
        var item = ItemTestDataBuilder.CreateRegularItem(100);
        var staticTile = CreateStaticTile(location, item);
        world.AddTile(staticTile, location);

        var itemFactoryMock = new Mock<IItemFactory>();
        itemFactoryMock.Setup(x => x.Create(
                It.IsAny<ushort>(),
                It.IsAny<Location>(),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IDictionary<ItemAttribute, IConvertible>>(),
                It.IsAny<IDictionary<string, IConvertible>>(),
                It.IsAny<IEnumerable<IItem>>()))
            .Returns((ushort serverId, Location loc, IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
                IDictionary<string, IConvertible> itemTypeCustomAttributes,
                IDictionary<ItemAttribute, IConvertible> itemAttributes,
                IDictionary<string, IConvertible> itemCustomAttributes, IEnumerable<IItem> children) =>
            {
                return ItemTestDataBuilder.CreateRegularItem(serverId);
            });

        var service = CreateService(
            itemFactory: itemFactoryMock.Object,
            world: world);

        // Act
        service.TransformIntoDynamicTile(staticTile);

        // Assert
        itemFactoryMock.Verify(x => x.Create(
                100,
                It.Is<Location>(l => l.X == 150 && l.Y == 250 && l.Z == 5),
                It.IsAny<IDictionary<ItemTypeAttribute, IConvertible>>()),
            Times.Once());
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Service_calls_tile_factory_with_correct_coordinate()
    {
        // Arrange
        var location = CreateLocation(123, 456, 9);
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location);
        world.AddTile(staticTile, location);

        var tileFactoryMock = new Mock<ITileFactory>();
        tileFactoryMock.Setup(x => x.CreateDynamicTile(
                It.IsAny<Coordinate>(),
                It.IsAny<TileFlag>(),
                It.IsAny<IItem[]>()))
            .Returns((Coordinate coordinate, TileFlag flag, IItem[] items) =>
            {
                return new DynamicTile(coordinate, flag, null, [], items);
            });

        var service = CreateService(
            tileFactory: tileFactoryMock.Object,
            world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        tileFactoryMock.Verify(x => x.CreateDynamicTile(
                It.Is<Coordinate>(c => c.X == 123 && c.Y == 456 && c.Z == 9),
                TileFlag.None,
                It.IsAny<IItem[]>()),
            Times.Once());
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Service_calls_world_replace_tile_with_new_dynamic_tile()
    {
        // Arrange
        var location = CreateLocation();
        var world = CreateWorld();
        var staticTile = CreateStaticTile(location);
        world.AddTile(staticTile, location);

        var tileFactoryMock = new Mock<ITileFactory>();
        var createdDynamicTile = CreateDynamicTile(location);

        tileFactoryMock.Setup(x => x.CreateDynamicTile(
                It.IsAny<Coordinate>(),
                It.IsAny<TileFlag>(),
                It.IsAny<IItem[]>()))
            .Returns(createdDynamicTile);

        var service = CreateService(
            tileFactory: tileFactoryMock.Object,
            world: world);

        // Act
        var result = service.TransformIntoDynamicTile(staticTile);

        // Assert
        result.Should().Be(createdDynamicTile);
        world.TryGetTile(ref location, out var tileInWorld).Should().BeTrue();
        tileInWorld.Should().Be(createdDynamicTile);
    }

    #endregion
}