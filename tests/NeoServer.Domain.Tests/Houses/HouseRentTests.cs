using Moq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseRentTests
{
    [Fact]
    public void PayRent_NotDue_ReturnsNotDue_NoDeduction()
    {
        var now = DateTime.UtcNow;
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 10000);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(10));

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.NotDue);
    }

    [Fact]
    public void PayRent_DueAndSufficientBank_DeductsRentAndAdvancesPaidUntil()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var bankMock = new Mock<IBank>();
        bankMock.Setup(x => x.Amount).Returns(10000);

        var playerMock = new Mock<IPlayer>();
        playerMock.Setup(x => x.Id).Returns(1u);
        playerMock.Setup(x => x.Bank).Returns(bankMock.Object);
        playerMock.Setup(x => x.BankAmount).Returns(10000);

        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1));

        var result = house.PayRent(playerMock.Object, now, 86400);

        result.Should().Be(HouseRentResult.Paid);
        bankMock.Verify(x => x.Debit(1000), Times.Once);
        house.PaidUntil.Should().Be(now.AddSeconds(86400));
    }

    [Fact]
    public void PayRent_DueAndSufficientBank_ResetsWarningsToZero()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 10000);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1), payRentWarnings: 3);

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.Paid);
        house.PayRentWarnings.Should().Be(0);
    }

    [Fact]
    public void PayRent_DueAndInsufficientBank_IncrementsWarnings()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1));

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.Warned);
        house.PayRentWarnings.Should().Be(1);
    }

    [Fact]
    public void PayRent_SeventhWarning_ReturnsEvicted()
    {
        var now = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, paidUntil: now.AddDays(-1), payRentWarnings: 6);

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.Evicted);
        house.OwnerGuid.Should().Be(0);
    }

    [Fact]
    public void PayRent_RentZero_ReturnsNotDue()
    {
        var now = DateTime.UtcNow;
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1, rent: 0, paidUntil: now.AddDays(-1));

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.NotDue);
    }

    [Fact]
    public void PayRent_Unowned_ReturnsNotDue()
    {
        var now = DateTime.UtcNow;
        var player = HouseTestDataBuilder.CreatePlayerWithBank(id: 1, bankAmount: 0);
        var house = HouseTestDataBuilder.Build();

        var result = house.PayRent(player, now, 86400);

        result.Should().Be(HouseRentResult.NotDue);
    }

    [Fact]
    public void SetPayRentWarnings_AboveCap_ClampsToSeven()
    {
        var house = HouseTestDataBuilder.Build();

        house.PayRentWarnings = 10;

        house.PayRentWarnings.Should().Be(7);
    }
}
