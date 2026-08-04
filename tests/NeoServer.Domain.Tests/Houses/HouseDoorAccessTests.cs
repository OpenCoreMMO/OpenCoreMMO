using NeoServer.Domain.Creatures.Player;
using NeoServer.Domain.Houses;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseDoorAccessTests
{
    [Fact]
    [Trait("Category", "HappyPath")]
    public void House_allows_door_use_when_player_is_owner()
    {
        var player = HouseTestDataBuilder.CreatePlayer(id: 1);
        var house = HouseTestDataBuilder.Build(ownerGuid: 1);

        house.CanUseDoor(player, doorId: 1).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void House_allows_door_use_when_player_is_subowner()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Sub");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.SubOwnerList, HouseTestDataBuilder.CreateAccessList("Sub") }
        });

        house.CanUseDoor(player, doorId: 1).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void House_denies_door_use_when_player_is_guest_only()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Guest");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { HouseListId.GuestList, HouseTestDataBuilder.CreateAccessList("Guest") }
        });

        house.CanUseDoor(player, doorId: 1).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void House_allows_door_use_when_player_is_on_that_door_list()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "DoorUser");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { 1, HouseTestDataBuilder.CreateAccessList("DoorUser") }
        });

        house.CanUseDoor(player, doorId: 1).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void House_denies_door_use_when_player_is_only_on_another_door_list()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "DoorUser");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { 2, HouseTestDataBuilder.CreateAccessList("DoorUser") }
        });

        house.CanUseDoor(player, doorId: 1).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void House_denies_door_use_when_door_list_is_null()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build();

        house.CanUseDoor(player, doorId: 1).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void House_denies_door_use_when_door_list_is_empty()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { 1, new HouseAccessList() }
        });

        house.CanUseDoor(player, doorId: 1).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void House_allows_door_use_when_player_has_group_access()
    {
        var group = new Group { Access = true };
        var player = HouseTestDataBuilder.CreatePlayer(name: "GM", group: group);
        var house = HouseTestDataBuilder.Build();

        house.CanUseDoor(player, doorId: 1).Should().BeTrue();
        house.GetAccessLevel(player).Should().Be(HouseAccessLevel.Owner);
    }

    [Fact]
    [Trait("Category", "HappyPath")]
    public void House_allows_door_use_when_player_is_on_door_list_zero()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "DoorUser");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { 0, HouseTestDataBuilder.CreateAccessList("DoorUser") }
        });

        house.CanUseDoor(player, doorId: 0).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Validation")]
    public void House_denies_door_use_when_stranger_uses_door_list_zero()
    {
        var player = HouseTestDataBuilder.CreatePlayer(name: "Stranger");
        var house = HouseTestDataBuilder.Build(accessLists: new Dictionary<uint, HouseAccessList>
        {
            { 0, HouseTestDataBuilder.CreateAccessList("DoorUser") }
        });

        house.CanUseDoor(player, doorId: 0).Should().BeFalse();
    }
}
