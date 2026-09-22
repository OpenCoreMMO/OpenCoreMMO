using Moq;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.Events;
using NeoServer.Domain.Houses.Services;
using NeoServer.Domain.Repositories;
using NeoServer.Domain.Tests.Helpers;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseServiceTests
{
    [Fact]
    public void SetOwner_EvictsNowUninvitedPlayers()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, level: 5);

        var tileMock = HouseTestDataBuilder.CreateTileMock(players: new List<IPlayer> { guest });
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            entryPosition: new Location(100, 100, 7));

        var evictionMock = new Mock<IHouseEviction>();
        var service = CreateService(eviction: evictionMock);

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        evictionMock.Verify(x => x.TeleportToExit(guest,
            It.Is<Location>(l => l.X == 100 && l.Y == 100 && l.Z == 7)), Times.Once);
    }

    [Fact]
    public void SetOwner_WakesAllBeds()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var bedMock = HouseTestDataBuilder.CreateItemMock();
        var tileMock = HouseTestDataBuilder.CreateTileMock();

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            beds: new List<Mock<IItem>> { bedMock });

        var bedWakerMock = new Mock<IHouseBedWaker>();
        var service = CreateService(bedWaker: bedWakerMock);

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        bedWakerMock.Verify(x => x.WakeAll(It.Is<IEnumerable<IItem>>(b => b.Contains(bedMock.Object))), Times.Once);
    }

    [Fact]
    public void SetOwner_TransfersPickupableItemsToOldOwnerDepot()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var pickupableItem = HouseTestDataBuilder.CreateItemMock(isPickupable: true);
        var nonPickupableItem = HouseTestDataBuilder.CreateItemMock(isPickupable: false);

        var tileMock = HouseTestDataBuilder.CreateTileMock(items: new List<IItem>
        {
            pickupableItem.Object,
            nonPickupableItem.Object
        });

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            ownerAccountId: 50,
            townId: 2,
            tiles: new List<Mock<IDynamicTile>> { tileMock });

        var depotMock = new Mock<IHouseDepotTransfer>();
        var service = CreateService(depotTransfer: depotMock);

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        depotMock.Verify(x => x.TransferToOwnerDepot(house, 1u), Times.Once);
    }

    [Fact]
    public void SetOwner_LeavesNonPickupableItems()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var nonPickupableItem = HouseTestDataBuilder.CreateItemMock(isPickupable: false);
        var tileMock = HouseTestDataBuilder.CreateTileMock(items: new List<IItem> { nonPickupableItem.Object });

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            ownerAccountId: 50,
            tiles: new List<Mock<IDynamicTile>> { tileMock });

        var depotMock = new Mock<IHouseDepotTransfer>();
        var service = CreateService(depotTransfer: depotMock);

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        depotMock.Verify(x => x.TransferToOwnerDepot(house, 1u), Times.Once);
    }

    [Fact]
    public void SetOwner_PersistsHouse()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        var repoMock = new Mock<IHouseRepository>();
        var service = CreateService(repo: repoMock);

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        repoMock.Verify(x => x.Save(house), Times.Once);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void SetOwner_ToZeroGuid_EvictsAllOccupantsIncludingOldOwner()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var owner = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);
        var guest = HouseTestDataBuilder.CreatePlayer(id: 2, level: 5);

        var tileMock = HouseTestDataBuilder.CreateTileMock(players: new List<IPlayer> { owner, guest });
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            entryPosition: new Location(100, 100, 7));

        var evictionMock = new Mock<IHouseEviction>();
        var service = CreateService(eviction: evictionMock);

        service.SetOwner(house, 0, null, 0, false, now, 86400);

        evictionMock.Verify(x => x.TeleportToExit(owner,
            It.Is<Location>(l => l.X == 100 && l.Y == 100 && l.Z == 7)), Times.Once);
        evictionMock.Verify(x => x.TeleportToExit(guest,
            It.Is<Location>(l => l.X == 100 && l.Y == 100 && l.Z == 7)), Times.Once);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void SetOwner_ToZeroGuid_MarksHouseUnownedAndPersists()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        HouseOwnerChangedEvent capturedEvent = null;
        EventAggregatorTestHelper.SetupEventAggregator<HouseOwnerChangedEvent>(e => capturedEvent = e);

        var house = HouseTestDataBuilder.Build(ownerGuid: 1, ownerName: "Owner", ownerAccountId: 50);
        var repoMock = new Mock<IHouseRepository>();
        var service = CreateService(repo: repoMock);

        service.SetOwner(house, 0, null, 0, false, now, 86400);

        house.OwnerGuid.Should().Be(0);
        house.OwnerName.Should().BeEmpty();
        house.OwnerAccountId.Should().Be(0);
        repoMock.Verify(x => x.Save(house), Times.Once);
        capturedEvent.Should().NotBeNull();
        capturedEvent.OldOwnerGuid.Should().Be(1);
        capturedEvent.NewOwnerGuid.Should().Be(0);
    }

    [Fact]
    public void SetOwner_RaisesHouseOwnerChangedEvent()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        HouseOwnerChangedEvent capturedEvent = null;
        EventAggregatorTestHelper.SetupEventAggregator<HouseOwnerChangedEvent>(e => capturedEvent = e);

        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var service = CreateService();

        service.SetOwner(house, 10, "NewOwner", 100, false, now, 86400);

        capturedEvent.Should().NotBeNull();
        capturedEvent.OldOwnerGuid.Should().Be(1);
        capturedEvent.NewOwnerGuid.Should().Be(10);
    }

    [Fact]
    public void PayRent_Warned_RaisesHouseRentWarningEvent()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        HouseRentWarningEvent capturedEvent = null;
        EventAggregatorTestHelper.SetupEventAggregator<HouseRentWarningEvent>(e => capturedEvent = e);

        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1));

        var depotMock = new Mock<IHouseDepotTransfer>();
        var service = CreateService(depotTransfer: depotMock);

        service.PayRent(house, player, now, 86400);

        capturedEvent.Should().NotBeNull();
        capturedEvent.House.Should().Be(house);
        capturedEvent.WarningNumber.Should().Be(1);
        depotMock.Verify(x => x.TransferToOwnerDepot(It.IsAny<House>(), It.IsAny<uint>()), Times.Never);
    }

    [Fact]
    public void PayRent_Evicted_RaisesHouseEvictedEvent()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        HouseEvictedEvent capturedEvent = null;
        EventAggregatorTestHelper.SetupEventAggregator<HouseEvictedEvent>(e => capturedEvent = e);

        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1), payRentWarnings: 6);

        var depotMock = new Mock<IHouseDepotTransfer>();
        var service = CreateService(depotTransfer: depotMock);

        service.PayRent(house, player, now, 86400);

        capturedEvent.Should().NotBeNull();
        capturedEvent.House.Should().Be(house);
        depotMock.Verify(x => x.TransferToOwnerDepot(house, player.Id), Times.Once);
    }

    [Fact]
    public void PayRent_Paid_PersistsHouseState()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 10000);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1));

        var repoMock = new Mock<IHouseRepository>();
        var service = CreateService(repo: repoMock);

        service.PayRent(house, player, now, 86400);

        repoMock.Verify(x => x.Save(house), Times.Once);
    }

    [Fact]
    public void CanKick_Success_TeleportsTargetWithoutPersisting()
    {
        var tileMock = HouseTestDataBuilder.CreateTileMock();
        var target = HouseTestDataBuilder.CreatePlayer(id: 2, level: 5);
        var targetMock = Mock.Get(target);
        targetMock.Setup(x => x.Tile).Returns(tileMock.Object);

        tileMock.Setup(x => x.Players).Returns(new List<IPlayer> { target });

        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            tiles: new List<Mock<IDynamicTile>> { tileMock },
            entryPosition: new Location(100, 100, 7));

        var caster = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);

        var evictionMock = new Mock<IHouseEviction>();
        var repoMock = new Mock<IHouseRepository>();
        var service = CreateService(eviction: evictionMock, repo: repoMock);

        service.KickPlayer(house, caster, target).Should().BeTrue();

        evictionMock.Verify(x => x.TeleportToExit(target,
            It.Is<Location>(l => l.X == 100 && l.Y == 100 && l.Z == 7)), Times.Once);
        repoMock.Verify(x => x.Save(It.IsAny<House>()), Times.Never);
    }

    [Fact]
    public void CanKick_Failure_NoSideEffects()
    {
        var target = HouseTestDataBuilder.CreatePlayer(id: 3, level: 5);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);
        var caster = HouseTestDataBuilder.CreatePlayer(id: 1, level: 100);

        var evictionMock = new Mock<IHouseEviction>();
        var repoMock = new Mock<IHouseRepository>();
        var service = CreateService(eviction: evictionMock, repo: repoMock);

        service.KickPlayer(house, caster, target).Should().BeFalse();

        evictionMock.Verify(x => x.TeleportToExit(It.IsAny<IPlayer>(), It.IsAny<Location>()), Times.Never);
        repoMock.Verify(x => x.Save(It.IsAny<House>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsTrueForNonPremiumPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: false));
        var player = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: false);

        service.CanPlayerOwnHouse(player).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsTrueForPremiumPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: false));
        var player = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: true);

        service.CanPlayerOwnHouse(player).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsFalseForNonPremiumPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: true));
        var player = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: false);

        service.CanPlayerOwnHouse(player).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsTrueForPremiumPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: true));
        var player = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: true);

        service.CanPlayerOwnHouse(player).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsTrue_ReturnsFalseForNullPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: true));

        service.CanPlayerOwnHouse(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void CanPlayerOwnHouse_WhenRequirePremiumIsFalse_ReturnsFalseForNullPlayer()
    {
        var service = CreateService(houseConfiguration: new HouseConfiguration(RequirePremiumAccount: false));

        service.CanPlayerOwnHouse(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void CanPlayerOwnHouse_DefaultConfig_RequiresPremium()
    {
        var service = CreateService();
        var freePlayer = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: false);
        var premiumPlayer = HouseTestDataBuilder.CreatePlayer(hasPremiumTime: true);

        service.CanPlayerOwnHouse(freePlayer).Should().BeFalse();
        service.CanPlayerOwnHouse(premiumPlayer).Should().BeTrue();
    }

    private static HouseService CreateService(
        Mock<IHouseRepository> repo = null,
        Mock<IHouseEviction> eviction = null,
        Mock<IHouseBedWaker> bedWaker = null,
        Mock<IHouseDepotTransfer> depotTransfer = null,
        HouseConfiguration houseConfiguration = null)
    {
        return new HouseService(
            repo?.Object ?? new Mock<IHouseRepository>().Object,
            eviction?.Object ?? new Mock<IHouseEviction>().Object,
            bedWaker?.Object ?? new Mock<IHouseBedWaker>().Object,
            depotTransfer?.Object ?? new Mock<IHouseDepotTransfer>().Object,
            houseConfiguration ?? new HouseConfiguration());
    }
}
