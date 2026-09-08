using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseStoreTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetByTile_returns_house_when_tile_has_house_id()
    {
        var house = HouseTestDataBuilder.Build(id: 5);
        var store = new HouseStore();
        store.AddOrUpdate(5, house);

        var tileMock = new Mock<IDynamicTile>();
        tileMock.Setup(t => t.HouseId).Returns(5u);

        store.GetByTile(tileMock.Object).Should().BeSameAs(house);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void GetByTile_returns_null_when_tile_has_no_house_id()
    {
        var house = HouseTestDataBuilder.Build(id: 5);
        var store = new HouseStore();
        store.AddOrUpdate(5, house);

        var tileMock = new Mock<IDynamicTile>();
        tileMock.Setup(t => t.HouseId).Returns((uint?)null);

        store.GetByTile(tileMock.Object).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void GetByTile_returns_null_when_house_id_is_zero()
    {
        var store = new HouseStore();

        var tileMock = new Mock<IDynamicTile>();
        tileMock.Setup(t => t.HouseId).Returns(0u);

        store.GetByTile(tileMock.Object).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetByOwnerGuid_returns_house_when_owner_matches()
    {
        var house = HouseTestDataBuilder.Build(id: 5, ownerGuid: 42);
        var store = new HouseStore();
        store.AddOrUpdate(5, house);

        store.GetByOwnerGuid(42).Should().BeSameAs(house);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void GetByOwnerGuid_returns_null_when_owner_is_unknown()
    {
        var house = HouseTestDataBuilder.Build(id: 5, ownerGuid: 42);
        var store = new HouseStore();
        store.AddOrUpdate(5, house);

        store.GetByOwnerGuid(99).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void GetByOwnerGuid_returns_null_when_owner_guid_is_zero()
    {
        var house = HouseTestDataBuilder.Build(id: 5, ownerGuid: 0);
        var store = new HouseStore();
        store.AddOrUpdate(5, house);

        store.GetByOwnerGuid(0).Should().BeNull();
    }
}
