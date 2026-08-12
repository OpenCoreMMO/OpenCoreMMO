using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Models.Tiles;
using NeoServer.Domain.World.Services;
using NeoServer.Server.Commands.Player.House;
using NeoServer.Server.Common.Contracts;
using Serilog;
using Xunit;

namespace NeoServer.Server.Tests.Commands;

public class PlayerKickFromHouseCommandTest
{
    private const uint HouseId = 10;
    private static readonly Location HouseTileLocation = new(50, 49, 7);
    private static readonly Location EntryLocation = new(50, 50, 7);
    private static readonly Location OutsideLocation = new(50, 48, 7);

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_SelfKick_TeleportsPlayerToHouseEntry()
    {
        // Arrange
        var (map, houseTile) = BuildMapWithHouseTile();
        var caster = PlayerTestDataBuilder.Build(id: 1, name: "Owner", level: 50, map: map);
        caster.SetNewLocation(HouseTileLocation);
        map.PlaceCreature(caster);

        var house = HouseTestDataBuilder.Build(
            id: HouseId,
            ownerGuid: 1,
            entryPosition: EntryLocation,
            realTiles: [houseTile]);

        var houseStore = CreateHouseStore(house);
        var creatureManager = CreateCreatureManager(caster);
        var command = CreateCommand(houseStore, map, creatureManager, out _);

        // Act
        command.Execute(caster, "Owner");

        // Assert
        caster.Location.Should().Be(EntryLocation);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_OwnerKicksGuest_TeleportsGuestToHouseEntry()
    {
        // Arrange
        var (map, houseTile) = BuildMapWithHouseTile();
        var guest = PlayerTestDataBuilder.Build(id: 2, name: "Guest", level: 500, map: map);
        guest.SetNewLocation(HouseTileLocation);
        map.PlaceCreature(guest);

        var house = HouseTestDataBuilder.Build(
            id: HouseId,
            ownerGuid: 1,
            entryPosition: EntryLocation,
            realTiles: [houseTile]);

        var houseStore = CreateHouseStore(house);
        var creatureManager = CreateCreatureManager(guest);
        var command = CreateCommand(houseStore, map, creatureManager, out var houseRepository);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", level: 500, map: map);

        // Act
        command.Execute(owner, "Guest");

        // Assert
        guest.Location.Should().Be(EntryLocation);
        houseRepository.Verify(x => x.Save(house), Times.Once);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void Execute_GuestKicksOtherGuest_TeleportsGuestToHouseEntry()
    {
        // Arrange
        var (map, houseTile) = BuildMapWithHouseTile();
        var target = PlayerTestDataBuilder.Build(id: 3, name: "Target", level: 10, map: map);
        target.SetNewLocation(HouseTileLocation);
        map.PlaceCreature(target);

        var house = HouseTestDataBuilder.Build(
            id: HouseId,
            ownerGuid: 1,
            entryPosition: EntryLocation,
            realTiles: [houseTile],
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest", "Target") }
            });

        var houseStore = CreateHouseStore(house);
        var creatureManager = CreateCreatureManager(target);
        var command = CreateCommand(houseStore, map, creatureManager, out var houseRepository);
        var guest = PlayerTestDataBuilder.Build(id: 4, name: "Guest", level: 5, map: map);

        // Act
        command.Execute(guest, "Target");

        // Assert
        target.Location.Should().Be(EntryLocation);
        houseRepository.Verify(x => x.Save(house), Times.Once);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Execute_GuestTriesToKickOwner_DoesNotTeleport()
    {
        // Arrange
        var (map, houseTile) = BuildMapWithHouseTile();
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", level: 10, map: map);
        owner.SetNewLocation(HouseTileLocation);
        map.PlaceCreature(owner);

        var house = HouseTestDataBuilder.Build(
            id: HouseId,
            ownerGuid: 1,
            entryPosition: EntryLocation,
            realTiles: [houseTile],
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        var houseStore = CreateHouseStore(house);
        var creatureManager = CreateCreatureManager(owner);
        var command = CreateCommand(houseStore, map, creatureManager, out var houseRepository);
        var guest = PlayerTestDataBuilder.Build(id: 4, name: "Guest", level: 5, map: map);

        // Act
        command.Execute(guest, "Owner");

        // Assert
        owner.Location.Should().Be(HouseTileLocation);
        houseRepository.Verify(x => x.Save(It.IsAny<Domain.Houses.House>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void Execute_TargetNotInsideHouse_DoesNotTeleport()
    {
        // Arrange
        var (map, houseTile) = BuildMapWithHouseTile();
        var target = PlayerTestDataBuilder.Build(id: 3, name: "Target", level: 5, map: map);
        target.SetNewLocation(OutsideLocation);
        map.PlaceCreature(target);

        var house = HouseTestDataBuilder.Build(
            id: HouseId,
            ownerGuid: 1,
            entryPosition: EntryLocation,
            realTiles: [houseTile]);

        var houseStore = CreateHouseStore(house);
        var creatureManager = CreateCreatureManager(target);
        var command = CreateCommand(houseStore, map, creatureManager, out var houseRepository);
        var owner = PlayerTestDataBuilder.Build(id: 1, name: "Owner", level: 100, map: map);

        // Act
        command.Execute(owner, "Target");

        // Assert
        target.Location.Should().Be(OutsideLocation);
        houseRepository.Verify(x => x.Save(It.IsAny<Domain.Houses.House>()), Times.Never);
    }

    private static (IMap Map, IDynamicTile HouseTile) BuildMapWithHouseTile()
    {
        var houseTile = new DynamicTile(new Coordinate(HouseTileLocation), TileFlag.None,
            MapTestDataBuilder.CreateGround(HouseTileLocation), [], [], houseId: HouseId);
        var entryTile = MapTestDataBuilder.CreateTile(EntryLocation);
        var outsideTile = MapTestDataBuilder.CreateTile(OutsideLocation);

        return (MapTestDataBuilder.Build(houseTile, entryTile, outsideTile), houseTile);
    }

    private static HouseStore CreateHouseStore(Domain.Houses.House house)
    {
        var houseStore = new HouseStore();
        houseStore.AddOrUpdate(house.Id, house);
        return houseStore;
    }

    private static Mock<IGameCreatureManager> CreateCreatureManager(IPlayer player)
    {
        var creatureManager = new Mock<IGameCreatureManager>();
        creatureManager
            .Setup(x => x.TryGetPlayer(It.IsAny<string>(), out It.Ref<IPlayer>.IsAny))
            .Returns((string name, out IPlayer target) =>
            {
                target = string.Equals(name, player.Name, StringComparison.OrdinalIgnoreCase) ? player : null;
                return target is not null;
            });

        return creatureManager;
    }

    private static PlayerKickFromHouseCommand CreateCommand(
        IHouseStore houseStore,
        IMap map,
        Mock<IGameCreatureManager> creatureManager,
        out Mock<IHouseRepository> houseRepository)
    {
        var eviction = new HouseEvictionService(CreateMovementService(map));
        houseRepository = new Mock<IHouseRepository>();

        var houseService = new HouseService(
            houseRepository.Object,
            eviction,
            new Mock<IHouseBedWaker>().Object,
            new Mock<IHouseDepotTransfer>().Object,
            new Mock<ICreatureGameInstance>().Object,
            new HouseConfiguration());

        return new PlayerKickFromHouseCommand(creatureManager.Object, houseStore, houseService);
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
}
