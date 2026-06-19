using Moq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseItemMovementPolicyTests
{
    [Fact]
    public void CanMoveItem_NonHouseTile_ReturnsTrue()
    {
        var storeMock = new Mock<IHouseStore>();
        var tileMock = new Mock<ITile>();
        storeMock.Setup(s => s.GetByTile(tileMock.Object)).Returns((House)null);

        var policy = new HouseItemMovementPolicy(storeMock.Object);
        var player = HouseTestDataBuilder.CreatePlayer();

        policy.CanMoveItem(player, tileMock.Object).Should().BeTrue();
    }

    [Fact]
    public void CanMoveItem_HouseTile_InvitedPlayer_ReturnsTrue()
    {
        var storeMock = new Mock<IHouseStore>();
        var tileMock = new Mock<ITile>();

        var ownerPlayer = HouseTestDataBuilder.CreatePlayer(id: 1);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        storeMock.Setup(s => s.GetByTile(tileMock.Object)).Returns(house);

        var policy = new HouseItemMovementPolicy(storeMock.Object);

        policy.CanMoveItem(ownerPlayer, tileMock.Object).Should().BeTrue();
    }

    [Fact]
    public void CanMoveItem_HouseTile_UninvitedPlayer_ReturnsFalse()
    {
        var storeMock = new Mock<IHouseStore>();
        var tileMock = new Mock<ITile>();

        var stranger = HouseTestDataBuilder.CreatePlayer(id: 99);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);  // owner is id=1, not id=99

        storeMock.Setup(s => s.GetByTile(tileMock.Object)).Returns(house);

        var policy = new HouseItemMovementPolicy(storeMock.Object);

        policy.CanMoveItem(stranger, tileMock.Object).Should().BeFalse();
    }

    [Fact]
    public void CanMoveItem_HouseTile_NullPlayer_ReturnsFalse()
    {
        var storeMock = new Mock<IHouseStore>();
        var tileMock = new Mock<ITile>();
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        storeMock.Setup(s => s.GetByTile(tileMock.Object)).Returns(house);

        var policy = new HouseItemMovementPolicy(storeMock.Object);

        policy.CanMoveItem(null, tileMock.Object).Should().BeFalse();
    }

    [Fact]
    public void CanMoveItem_NullTile_ReturnsTrue()
    {
        var storeMock = new Mock<IHouseStore>();
        var policy = new HouseItemMovementPolicy(storeMock.Object);
        var player = HouseTestDataBuilder.CreatePlayer();

        policy.CanMoveItem(player, null).Should().BeTrue();
    }
}
