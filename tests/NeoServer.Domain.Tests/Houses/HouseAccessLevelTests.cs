using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Tests.Helpers.House;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;

namespace NeoServer.Domain.Tests.Houses;

public class HouseAccessLevelTests
{
    [Fact]
    public void GetAccessLevel_Owner_ReturnsOwner()
    {
        var player = HouseTestDataBuilder.CreatePlayer(id: 1);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.Owner);
    }

    [Fact]
    public void GetAccessLevel_PlayerInSubownerList_ReturnsSubOwner()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Sub");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Sub") }
        });

        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.SubOwner);
    }

    [Fact]
    public void GetAccessLevel_PlayerInGuestListOnly_ReturnsGuest()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Guest");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
        });

        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.Guest);
    }

    [Fact]
    public void GetAccessLevel_PlayerInBothLists_ReturnsSubOwner()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Player");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Player") },
            { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Player") }
        });

        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.SubOwner);
    }

    [Fact]
    public void GetAccessLevel_UninvitedPlayer_ReturnsNotInvited()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build();

        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.NotInvited);
    }

    [Fact]
    public void IsInvited_GuestPlayer_ReturnsTrue()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Guest");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
        });

        house.IsInvited(player).Should().BeTrue();
    }

    [Fact]
    public void IsInvited_UninvitedPlayer_ReturnsFalse()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build();

        house.IsInvited(player).Should().BeFalse();
    }

    [Fact]
    public void CanEnter_InvitedPlayer_ReturnsTrue()
    {
        var player = HouseTestDataBuilder.CreatePlayer(id: 1);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        house.CanEnter(player).Should().BeTrue();
    }

    [Fact]
    public void CanEnter_UninvitedPlayer_ReturnsFalse()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build();

        house.CanEnter(player).Should().BeFalse();
    }

    [Fact]
    public void CanEnter_NonPlayerCreature_ReturnsFalse()
    {
        var creatureMock = new Moq.Mock<ICreature>();
        var house = HouseTestDataBuilder.Build();

        house.CanEnter(creatureMock.Object).Should().BeFalse();
    }

    [Fact]
    public void CanEditAccessList_OwnerEditsSubownerList_ReturnsTrue()
    {
        var player = HouseTestDataBuilder.CreatePlayer(id: 1);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        house.CanEditAccessList(HouseListId.SubOwnerList, player).Should().BeTrue();
    }

    [Fact]
    public void CanEditAccessList_SubownerEditsSubownerList_ReturnsFalse()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Sub");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Sub") }
        });

        house.CanEditAccessList(HouseListId.SubOwnerList, player).Should().BeFalse();
    }

    [Fact]
    public void CanEditAccessList_SubownerEditsGuestList_ReturnsTrue()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Sub");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Sub") }
        });

        house.CanEditAccessList(HouseListId.GuestList, player).Should().BeTrue();
    }

    [Fact]
    public void CanEditAccessList_GuestEditsAnyList_ReturnsFalse()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Guest");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
        });

        house.CanEditAccessList(HouseListId.GuestList, player).Should().BeFalse();
        house.CanEditAccessList(HouseListId.SubOwnerList, player).Should().BeFalse();
        house.CanEditAccessList(1, player).Should().BeFalse();
    }
}
