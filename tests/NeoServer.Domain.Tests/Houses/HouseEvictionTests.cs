using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseEvictionTests
{
    [Fact]
    public void CanKick_OwnerKicksGuest_ReturnsTrue()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, level: 5);
        var guestMock = Mock.Get(guest);
        guestMock.Setup(x => x.Tile).Returns(tileMock.Object);

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock });

        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);

        house.CanKick(owner, guest).Should().BeTrue();
    }

    [Fact]
    public void CanKick_SubownerKicksGuest_ReturnsTrue()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var guest = HouseTestDataBuilder.CreatePlayer(id: 3, level: 5);
        var guestMock = Mock.Get(guest);
        guestMock.Setup(x => x.Tile).Returns(tileMock.Object);

        var house = HouseTestDataBuilder.Build(
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Sub") }
            });

        var subowner = HouseTestDataBuilder.CreatePlayer(id: 2, level: 50, name: "Sub");

        house.CanKick(subowner, guest).Should().BeTrue();
    }

    [Fact]
    public void CanKick_GuestKicksAnyone_ReturnsFalse()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var target = HouseTestDataBuilder.CreatePlayer(id: 2, level: 5);
        var targetMock = Mock.Get(target);
        targetMock.Setup(x => x.Tile).Returns(tileMock.Object);

        var house = HouseTestDataBuilder.Build(
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        var guestKicker = HouseTestDataBuilder.CreatePlayer(id: 3, level: 10, name: "Guest");

        house.CanKick(guestKicker, target).Should().BeFalse();
    }

    [Fact]
    public void CanKick_AnyoneKicksOwner_ReturnsFalse()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock });

        var subowner = HouseTestDataBuilder.CreatePlayer(id: 2, level: 50, name: "Sub");

        house.CanKick(subowner, owner).Should().BeFalse();
    }

    [Fact]
    public void CanKick_TargetNotInHouse_ReturnsFalse()
    {
        var target = HouseTestDataBuilder.CreatePlayer(id: 3, level: 5);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);

        house.CanKick(owner, target).Should().BeFalse();
    }
}
