using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseOwnershipTests
{
    [Fact]
    public void SetNewOwner_FromUnownedToPlayer_SetsOwnerGuidNameAccount()
    {
        var house = HouseTestDataBuilder.Build();

        house.SetNewOwner(10, "NewOwner", 100, false, DateTime.UtcNow, 86400);

        house.OwnerGuid.Should().Be(10);
        house.OwnerName.Should().Be("NewOwner");
        house.OwnerAccountId.Should().Be(100);
    }

    [Fact]
    public void SetNewOwner_WithUpdatePaidUntilTrue_SetsPaidUntilToNowPlusRentPeriod()
    {
        var house = HouseTestDataBuilder.Build();
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        house.SetNewOwner(10, "Owner", 100, true, now, 86400);

        house.PaidUntil.Should().Be(now.AddSeconds(86400));
    }

    [Fact]
    public void SetNewOwner_WithUpdatePaidUntilTrue_ResetsPayRentWarningsToZero()
    {
        var house = HouseTestDataBuilder.Build(payRentWarnings: 5);

        house.SetNewOwner(10, "Owner", 100, true, DateTime.UtcNow, 86400);

        house.PayRentWarnings.Should().Be(0);
    }

    [Fact]
    public void SetNewOwner_ChangingOwner_ClearsGuestAndSubownerLists()
    {
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") },
                { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("SubOwner") }
            });

        house.SetNewOwner(10, "NewOwner", 100, false, DateTime.UtcNow, 86400);

        house.GetAccessList(HouseListId.GuestList).Should().BeNull();
        house.GetAccessList(HouseListId.SubOwnerList).Should().BeNull();
    }

    [Fact]
    public void SetNewOwner_ChangingOwner_ClearsAllDoorAccessLists()
    {
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { 1, HouseTestDataBuilder.CreateAccessList("Door1") },
                { 2, HouseTestDataBuilder.CreateAccessList("Door2") }
            });

        house.SetNewOwner(10, "NewOwner", 100, false, DateTime.UtcNow, 86400);

        house.GetAccessList(1).Should().BeNull();
        house.GetAccessList(2).Should().BeNull();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void SetNewOwner_ChangingOwner_KeepsLinkedDoors()
    {
        var door = HouseTestDataBuilder.CreateItemMock(serverId: 1219);
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            doors: [(1, door)]);

        house.SetNewOwner(10, "NewOwner", 100, false, DateTime.UtcNow, 86400);

        house.DoorCount.Should().Be(1);
        house.Doors[1].Should().BeSameAs(door.Object);
    }

    [Fact]
    public void SetNewOwner_ToZeroGuid_MarksHouseUnowned()
    {
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            ownerName: "OldOwner",
            ownerAccountId: 50,
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
            });

        house.SetNewOwner(0, null, 0, false, DateTime.UtcNow, 86400);

        house.OwnerGuid.Should().Be(0);
        house.OwnerName.Should().BeEmpty();
        house.OwnerAccountId.Should().Be(0);
        house.GetAccessList(HouseListId.GuestList).Should().BeNull();
    }

    [Fact]
    public void SetNewOwner_ToSameGuid_DoesNotClearLists()
    {
        var accessList = HouseTestDataBuilder.CreateAccessList("Guest");
        var house = HouseTestDataBuilder.Build(
            ownerGuid: 1,
            ownerName: "Owner",
            ownerAccountId: 50,
            accessLists: new Dictionary<uint, HouseAccessList>
            {
                { HouseListId.GuestList, accessList }
            });

        house.SetNewOwner(1, "Owner", 50, false, DateTime.UtcNow, 86400);

        house.OwnerGuid.Should().Be(1);
        house.GetAccessList(HouseListId.GuestList).Should().BeSameAs(accessList);
    }

    [Fact]
    public void SetNewOwner_SameGuidWithUpdatePaidUntil_RefreshesPaidUntil()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now);

        house.SetNewOwner(1, "Owner", 50, true, now, 86400);

        house.PaidUntil.Should().Be(now.AddSeconds(86400));
    }
}
