using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Loaders.Houses;
using NeoServer.Server.Commands.Player.House;
using Xunit;

namespace NeoServer.Server.Tests.Commands;

public class PlayerEditHouseAccessListCommandTest
{
    [Fact]
    public void Execute_GuestListRemovesOnlineGuest_KicksUninvitedPlayer()
    {
        // Arrange
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, name: "Guest");
        var guestMock = Mock.Get(guest);
        var tile = HouseTestDataBuilder.CreateTileMock(players: [guest]);
        guestMock.Setup(x => x.Tile).Returns(tile.Object);

        var guestList = HouseTestDataBuilder.CreateAccessList("Guest");
        guestList.RawText = "Guest";

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            ownerName: "Owner",
            entryPosition: new Location(50, 50, 7),
            tiles: [tile],
            accessLists: new Dictionary<uint, Domain.Houses.AccessList.HouseAccessList>
            {
                { HouseListId.GuestList, guestList }
            });

        var houseStore = new Mock<IHouseStore>();
        houseStore.Setup(x => x.GetByHouseId(10)).Returns(house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new Mock<IHouseEviction>();
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction.Object);

        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, name: "Owner");

        // Act
        command.Execute(owner, houseId: 10, HouseListId.GuestList, text: string.Empty);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.GuestList, string.Empty), Times.Once);
        eviction.Verify(
            x => x.TeleportToExit(guest, It.Is<Location>(l => l.X == 50 && l.Y == 50 && l.Z == 7)),
            Times.Once);
    }

    [Fact]
    public void Execute_DoorListEdit_DoesNotKick()
    {
        // Arrange
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, name: "Guest");
        var tile = HouseTestDataBuilder.CreateTileMock(players: [guest]);

        var doorList = HouseTestDataBuilder.CreateAccessList("Guest");
        doorList.RawText = "Guest";

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            tiles: [tile],
            accessLists: new Dictionary<uint, Domain.Houses.AccessList.HouseAccessList>
            {
                { 1, doorList }
            });

        var houseStore = new Mock<IHouseStore>();
        houseStore.Setup(x => x.GetByHouseId(10)).Returns(house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new Mock<IHouseEviction>();
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction.Object);
        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, name: "Owner");

        // Act
        command.Execute(owner, houseId: 10, listId: 1, text: string.Empty);

        // Assert
        eviction.Verify(x => x.TeleportToExit(It.IsAny<IPlayer>(), It.IsAny<Location>()), Times.Never);
        houseRepository.Verify(x => x.SaveAccessList(10, 1, string.Empty), Times.Once);
    }

    [Fact]
    public void Execute_PlayerCannotEdit_DoesNotSave()
    {
        // Arrange
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);
        var houseStore = new Mock<IHouseStore>();
        houseStore.Setup(x => x.GetByHouseId(10)).Returns(house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new Mock<IHouseEviction>();
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction.Object);
        var stranger = HouseTestDataBuilder.CreatePlayer(id: 99, name: "Stranger");

        // Act
        command.Execute(stranger, houseId: 10, HouseListId.GuestList, text: "Someone");

        // Assert
        houseRepository.Verify(
            x => x.SaveAccessList(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>()),
            Times.Never);
        eviction.Verify(x => x.TeleportToExit(It.IsAny<IPlayer>(), It.IsAny<Location>()), Times.Never);
    }

    [Fact]
    public void HouseEditWindowStore_MismatchedWindowId_ReturnsFalse()
    {
        var player = HouseTestDataBuilder.CreatePlayer(id: 5, name: "Owner");
        var windowId = HouseEditWindowStore.SetEditHouse(player, houseId: 1, listId: HouseListId.GuestList);

        HouseEditWindowStore.TryGet(player, windowId + 1, out _, out _).Should().BeFalse();
        HouseEditWindowStore.TryGet(player, windowId, out var houseId, out var listId).Should().BeTrue();
        houseId.Should().Be(1u);
        listId.Should().Be(HouseListId.GuestList);

        HouseEditWindowStore.Clear(player);
    }

    private static PlayerEditHouseAccessListCommand CreateCommand(
        IHouseStore houseStore,
        IHouseRepository houseRepository,
        IHouseEviction eviction)
    {
        return new PlayerEditHouseAccessListCommand(
            houseStore,
            houseRepository,
            new HouseAccessListLoader(),
            new HouseConfiguration(),
            eviction);
    }
}
