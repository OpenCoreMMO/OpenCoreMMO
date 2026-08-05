using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;
using NeoServer.Loaders.Houses;
using NeoServer.Server.Commands.Player.House;
using Serilog;
using Xunit;

namespace NeoServer.Server.Tests.Commands;

public class PlayerEditHouseAccessListCommandTest
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_GuestListRemovesOnlineGuest_KicksUninvitedPlayer()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var guest = PlayerTestDataBuilder.Build(id: 2, name: "Guest", map: map);
        var guestTile = (IDynamicTile)map[50, 49, 7];

        var guestList = HouseTestDataBuilder.CreateAccessList("Guest");
        guestList.RawText = "Guest";

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            entryPosition: new Location(50, 50, 7),
            realTiles: [guestTile],
            accessLists: new Dictionary<uint, Domain.Houses.AccessList.HouseAccessList>
            {
                { HouseListId.GuestList, guestList }
            });

        guest.SetNewLocation(new Location(50, 49, 7));
        map.PlaceCreature(guest);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);

        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        // Act
        command.Execute(owner, houseId: 10, HouseListId.GuestList, text: string.Empty);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.GuestList, string.Empty), Times.Once);
        guest.Location.Should().Be(new Location(50, 50, 7));
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_SubOwnerListRemovesOnlineSubOwner_KicksRemovedPlayer()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var subOwner = PlayerTestDataBuilder.Build(id: 2, name: "SubOwner", map: map, premiumTime: 30);
        var subOwnerTile = (IDynamicTile)map[50, 49, 7];

        var subOwnerList = HouseTestDataBuilder.CreateAccessList("SubOwner");
        subOwnerList.RawText = "SubOwner";

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            entryPosition: new Location(50, 50, 7),
            realTiles: [subOwnerTile],
            accessLists: new Dictionary<uint, Domain.Houses.AccessList.HouseAccessList>
            {
                { HouseListId.SubOwnerList, subOwnerList }
            });

        subOwner.SetNewLocation(new Location(50, 49, 7));
        map.PlaceCreature(subOwner);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);

        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        // Act
        command.Execute(owner, houseId: 10, HouseListId.SubOwnerList, text: string.Empty);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.SubOwnerList, string.Empty), Times.Once);
        subOwner.Location.Should().Be(new Location(50, 50, 7));
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_DoorListEdit_DoesNotKick()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var guest = PlayerTestDataBuilder.Build(id: 2, name: "Guest", map: map);
        var guestTile = (IDynamicTile)map[50, 49, 7];

        var doorList = HouseTestDataBuilder.CreateAccessList("Guest");
        doorList.RawText = "Guest";

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            entryPosition: new Location(50, 50, 7),
            realTiles: [guestTile],
            accessLists: new Dictionary<uint, Domain.Houses.AccessList.HouseAccessList>
            {
                { 1, doorList }
            });

        guest.SetNewLocation(new Location(50, 49, 7));
        map.PlaceCreature(guest);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        // Act
        command.Execute(owner, houseId: 10, listId: 1, text: string.Empty);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, 1, string.Empty), Times.Once);
        guest.Location.Should().Be(new Location(50, 49, 7));
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Execute_SubOwnerListWithElevenEntries_DoesNotSave()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        var names = new List<string>(11);
        for (var i = 1; i <= 11; i++)
        {
            names.Add($"Sub{i}");
        }

        // Act
        command.Execute(owner, houseId: 10, HouseListId.SubOwnerList, string.Join('\n', names));

        // Assert
        houseRepository.Verify(
            x => x.SaveAccessList(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_SubOwnerListWithTenEntries_Saves()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        var names = new List<string>(10);
        for (var i = 1; i <= 10; i++)
        {
            names.Add($"Sub{i}");
        }
        var text = string.Join('\n', names);

        // Act
        command.Execute(owner, houseId: 10, HouseListId.SubOwnerList, text);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.SubOwnerList, text), Times.Once);
    }

    [Fact]
    [Trait("Category", "EdgeCase")]
    public void Execute_SubOwnerListExclusionsDoNotCountTowardLimit_Saves()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", map: map);

        var lines = new List<string>(12) { "!Excluded", "# comment" };
        for (var i = 1; i <= 10; i++)
        {
            lines.Add($"Sub{i}");
        }
        var text = string.Join('\n', lines);

        // Act
        command.Execute(owner, houseId: 10, HouseListId.SubOwnerList, text);

        // Assert
        houseRepository.Verify(x => x.SaveAccessList(10, HouseListId.SubOwnerList, text), Times.Once);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Execute_PlayerCannotEdit_DoesNotSave()
    {
        // Arrange
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var house = HouseTestDataBuilder.Build(id: 10, ownerGuid: 1);

        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(10, house);

        var houseRepository = new Mock<IHouseRepository>();
        var eviction = new HouseEvictionService(CreateMovementService(map));
        var command = CreateCommand(houseStore, houseRepository.Object, eviction);
        var stranger = PlayerTestDataBuilder.Build(id: 99, name: "Stranger", map: map);

        // Act
        command.Execute(stranger, houseId: 10, HouseListId.GuestList, text: "Someone");

        // Assert
        houseRepository.Verify(
            x => x.SaveAccessList(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void HouseEditWindowStore_MismatchedWindowId_ReturnsFalse()
    {
        var store = new HouseEditWindowStore();
        var player = PlayerTestDataBuilder.Build(id: 5, name: "Owner");
        var windowId = store.SetEditHouse(player, houseId: 1, listId: HouseListId.GuestList);

        store.TryGet(player, windowId + 1, out _, out _).Should().BeFalse();
        store.TryGet(player, windowId, out var houseId, out var listId).Should().BeTrue();
        houseId.Should().Be(1u);
        listId.Should().Be(HouseListId.GuestList);

        store.Clear(player);
    }

    private static CreatureMovementService CreateMovementService(IMap map)
    {
        var staticToDynamicTileService = new StaticToDynamicTileService(
            new ItemClientServerIdMapStore(),
            ItemFactoryTestBuilder.Build(),
            new TileFactory(new Mock<ILogger>().Object),
            new Domain.World.World());

        return new CreatureMovementService(
            map,
            new CylinderOperation(map),
            new CreatureMovementValidation(map),
            staticToDynamicTileService);
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
