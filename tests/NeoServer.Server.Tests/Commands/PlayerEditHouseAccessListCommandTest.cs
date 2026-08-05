using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
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
    [Trait("Category", "HappyPath")]
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
        var movementService = new FakeCreatureMovementService();
        var eviction = new HouseEvictionService(movementService);
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction);

        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, name: "Owner");

        // Act
        command.Execute(owner, houseId: 10, HouseListId.GuestList, text: string.Empty);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.GuestList, string.Empty), Times.Once);

        var teleport = movementService.TeleportedPlayers.Should().ContainSingle().Subject;
        teleport.Player.Should().BeSameAs(guest);
        teleport.Location.Should().Be(new Location(50, 50, 7));
    }

    [Fact]
    [Trait("Category", "HappyPath")]
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
        var movementService = new FakeCreatureMovementService();
        var eviction = new HouseEvictionService(movementService);
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction);
        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, name: "Owner");

        // Act
        command.Execute(owner, houseId: 10, listId: 1, text: string.Empty);

        // Assert
        movementService.TeleportedPlayers.Should().BeEmpty();
        houseRepository.Verify(x => x.SaveAccessList(10, 1, string.Empty), Times.Once);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Execute_PlayerCannotEdit_DoesNotSave()
    {
        // Arrange
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);
        var houseStore = new Mock<IHouseStore>();
        houseStore.Setup(x => x.GetByHouseId(10)).Returns(house);

        var houseRepository = new Mock<IHouseRepository>();
        var movementService = new FakeCreatureMovementService();
        var eviction = new HouseEvictionService(movementService);
        var command = CreateCommand(houseStore.Object, houseRepository.Object, eviction);
        var stranger = HouseTestDataBuilder.CreatePlayer(id: 99, name: "Stranger");

        // Act
        command.Execute(stranger, houseId: 10, HouseListId.GuestList, text: "Someone");

        // Assert
        houseRepository.Verify(
            x => x.SaveAccessList(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>()),
            Times.Never);
        movementService.TeleportedPlayers.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void HouseEditWindowStore_MismatchedWindowId_ReturnsFalse()
    {
        var store = new HouseEditWindowStore();
        var player = HouseTestDataBuilder.CreatePlayer(id: 5, name: "Owner");
        var windowId = store.SetEditHouse(player, houseId: 1, listId: HouseListId.GuestList);

        store.TryGet(player, windowId + 1, out _, out _).Should().BeFalse();
        store.TryGet(player, windowId, out var houseId, out var listId).Should().BeTrue();
        houseId.Should().Be(1u);
        listId.Should().Be(HouseListId.GuestList);

        store.Clear(player);
    }

    /// <summary>
    ///     Stub movement service that records teleport requests without moving
    ///     anything, so tests can assert on the real eviction flow.
    /// </summary>
    private sealed class FakeCreatureMovementService : ICreatureMovementService
    {
        public List<(IPlayer Player, Location Location)> TeleportedPlayers { get; } = [];

        public bool MoveCreature(IWalkableCreature creature, Direction nextDirection) => true;

        public void MoveCreature(IWalkableCreature creature)
        {
        }

        public bool MoveCreature(ICreature creature, Location location, bool forced = false, bool isTeleport = false)
        {
            if (isTeleport && creature is IPlayer player)
            {
                TeleportedPlayers.Add((player, location));
            }

            return true;
        }
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
