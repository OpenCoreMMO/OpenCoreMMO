using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.Tests.Helpers.Player;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;
using Serilog;

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
    public void CanKick_NonPremiumSubownerKicksGuest_ReturnsFalse()
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

        var subowner = HouseTestDataBuilder.CreatePlayer(id: 2, level: 50, name: "Sub", hasPremiumTime: false);

        house.CanKick(subowner, guest).Should().BeFalse();
    }

    [Fact]
    public void CanKick_NonPremiumSubownerKicksGuest_WhenPremiumNotRequired_ReturnsTrue()
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
        house.RequirePremiumForSubOwners = false;

        var subowner = HouseTestDataBuilder.CreatePlayer(id: 2, level: 50, name: "Sub", hasPremiumTime: false);

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

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetUninvitedOccupants_GuestRemovedFromList_ReturnsGuest()
    {
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, name: "Guest");
        var tileMock = HouseTestDataBuilder.CreateTileMock(players: [guest]);

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList() }
            });

        house.GetUninvitedOccupants().Should().ContainSingle().Which.Should().BeSameAs(guest);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void GetUninvitedOccupants_AllOccupantsStillInvited_ReturnsEmpty()
    {
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, name: "Guest");
        var tileMock = HouseTestDataBuilder.CreateTileMock(players: [guest]);

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        house.GetUninvitedOccupants().Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void KickUninvited_GuestListRemovesOnlineGuest_TeleportsToExit()
    {
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var guest = PlayerTestDataBuilder.Build(id: 2, name: "Guest", map: map);
        var guestTile = (IDynamicTile)map[50, 49, 7];

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            entryPosition: new Location(50, 50, 7),
            realTiles: [guestTile],
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        guest.SetNewLocation(new Location(50, 49, 7));
        map.PlaceCreature(guest);

        // Simulate the guest being removed from the list after the edit.
        house.GetAccessList(HouseListId.GuestList).Clear();

        var eviction = new HouseEvictionService(CreateMovementService(map));

        eviction.KickUninvited(house, HouseListId.GuestList);

        guest.Location.Should().Be(new Location(50, 50, 7));
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void KickUninvited_DoorListEdit_DoesNotTeleport()
    {
        var map = MapTestDataBuilder.Build(1, 101, 1, 101, 7, 7);
        var guest = PlayerTestDataBuilder.Build(id: 2, name: "Guest", map: map);
        var guestTile = (IDynamicTile)map[50, 49, 7];

        var house = HouseTestDataBuilder.Build(
            id: 10,
            ownerGuid: 1,
            entryPosition: new Location(50, 50, 7),
            realTiles: [guestTile],
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        guest.SetNewLocation(new Location(50, 49, 7));
        map.PlaceCreature(guest);

        // Even with the guest no longer invited, door-list edits must not kick.
        house.GetAccessList(HouseListId.GuestList).Clear();

        var eviction = new HouseEvictionService(CreateMovementService(map));

        eviction.KickUninvited(house, listId: 1);

        guest.Location.Should().Be(new Location(50, 49, 7));
    }

    private static CreatureMovementService CreateMovementService(IMap map)
    {
        var staticToDynamicTileService = new StaticToDynamicTileService(
            new ItemClientServerIdMapStore(),
            ItemFactoryTestBuilder.Build(),
            new TileFactory(new Mock<ILogger>().Object),
            new NeoServer.Domain.World.World());

        return new CreatureMovementService(
            map,
            new CylinderOperation(map),
            new CreatureMovementValidation(map),
            staticToDynamicTileService);
    }
}
