using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Tests.Helpers.Player;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players;

public class PlayerVipListTests
{
    [Fact]
    public void LoadVipList_Null_Do_Nothing()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        var called = false;
        sut.Vip.OnLoadedVipList += (_, _) => { called = true; };
        sut.Vip.LoadVipList(null);

        Assert.Empty(sut.Vip.VipList);
        Assert.False(called);
    }

    [Fact]
    public void LoadVipList_Empty_CallLoadedEvent()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);

        var called = false;
        sut.Vip.OnLoadedVipList += (_, _) => { called = true; };

        sut.Vip.LoadVipList(Array.Empty<(uint, string)>().AsEnumerable());

        Assert.Empty(sut.Vip.VipList);
        Assert.True(called);
    }

    [Fact]
    public void LoadVipList_2DuplicatedItems_FillWith1EntryVipList_CallLoadedEvent()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var vipToAdd = new List<(uint, string)>
        {
            (1, "test"),
            (1, "test")
        };
        var called = false;
        sut.Vip.OnLoadedVipList += (_, _) => { called = true; };

        sut.Vip.LoadVipList(vipToAdd);

        Assert.Single(sut.Vip.VipList);
        Assert.Equal((uint)1, sut.Vip.VipList.FirstOrDefault());
        Assert.True(called);
    }

    [Fact]
    public void LoadVipList_2Items_FillVipList_CallLoadedEventSendingThem()
    {
        var sut = PlayerTestDataBuilder.Build(hp: 100);
        var vipToAdd = new List<(uint, string)>
        {
            (1, "test"),
            (2, "test 2")
        };
        var called = false;
        IEnumerable<(uint, string)> vipSentEvent = null;
        sut.Vip.OnLoadedVipList += (_, viplist) =>
        {
            called = true;
            vipSentEvent = viplist;
        };

        sut.Vip.LoadVipList(vipToAdd);

        sut.Vip.VipList.Should().Contain(1);
        sut.Vip.VipList.Should().Contain(2);
        sut.Vip.VipList.Should().HaveCount(2);
        vipSentEvent.Should().Contain((1, "test"));
        vipSentEvent.Should().Contain((2, "test 2"));
        vipSentEvent.Should().HaveCount(2);
        Assert.True(called);
    }

    [Fact]
    public void HasInVipList_EmptyVipList_ReturnsFalse()
    {
        var sut = PlayerTestDataBuilder.Build();
        var result = sut.Vip.HasInVipList(1);
        result.Should().BeFalse();
    }

    [Fact]
    public void HasInVipList_ItemIsNotInVipList_ReturnsFalse()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.Vip.LoadVipList(new (uint, string)[] { (1, "test") });
        var result = sut.Vip.HasInVipList(2);
        result.Should().BeFalse();
    }

    [Fact]
    public void HasInVipList_ItemInVipList_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        sut.Vip.LoadVipList(new (uint, string)[] { (1, "test") });
        var result = sut.Vip.HasInVipList(1);
        result.Should().BeTrue();
    }

    [Fact]
    public void AddToVip_Null_DoNothing()
    {
        var sut = PlayerTestDataBuilder.Build();

        var eventCalled = false;
        sut.Vip.OnAddedToVipList += (_, _, _) => { eventCalled = true; };
        var result = sut.Vip.AddToVip(null);

        result.Should().BeFalse();
        sut.Vip.VipList.Should().BeEmpty();
        eventCalled.Should().BeFalse();
    }

    [Fact]
    public void AddToVip_PlayerAlreadyOnVip_ReturnsFalse()
    {
        var sut = PlayerTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build(2);


        var eventCalled = false;
        sut.Vip.OnAddedToVipList += (_, _, _) => { eventCalled = true; };

        sut.Vip.LoadVipList(new (uint, string)[] { (2, "player1") });
        var result = sut.Vip.AddToVip(player);

        result.Should().BeFalse();
        sut.Vip.VipList.Should().ContainSingle();
        eventCalled.Should().BeFalse();
    }

    [Fact]
    public void AddToVip_Player_AddToVip_ReturnsTrue()
    {
        var sut = PlayerTestDataBuilder.Build();
        var player = PlayerTestDataBuilder.Build(2, 1, "Player X");

        sut.Vip.LoadVipList(new (uint, string)[] { (3, "player1") });

        var eventCalled = false;
        (uint, string) playerAdded = (0, string.Empty);
        sut.Vip.OnAddedToVipList += (_, id_, name) =>
        {
            eventCalled = true;
            playerAdded.Item1 = id_;
            playerAdded.Item2 = name;
        };
        var result = sut.Vip.AddToVip(player);

        result.Should().BeTrue();
        sut.Vip.VipList.Should().HaveCount(2);
        eventCalled.Should().BeTrue();
        playerAdded.Should().Be((2, "Player X"));
    }

    [Fact]
    public void RemoveFromVip_PlayerNotInList_DoNothing()
    {
        var sut = PlayerTestDataBuilder.Build();

        sut.Vip.LoadVipList(new (uint, string)[] { (3, "player1") });

        sut.Vip.RemoveFromVip(2);
        sut.Vip.VipList.Should().ContainSingle();
    }

    [Fact]
    public void RemoveFromVip_RemovesFromList()
    {
        var sut = PlayerTestDataBuilder.Build();

        sut.Vip.LoadVipList(new (uint, string)[] { (2, "player1"), (3, "player1") });

        sut.Vip.RemoveFromVip(3);
        sut.Vip.VipList.Should().ContainSingle();
        sut.Vip.VipList.Should().BeEquivalentTo(new uint[] { 2 });
    }

    [Fact]
    public void Add_more_than_200_friends_is_not_allowed()
    {
        //arrange

        var friends = new Fixture().CreateMany<(uint, string)>(200).ToArray();
        var sut = PlayerTestDataBuilder.Build();
        var newFriend = PlayerTestDataBuilder.Build(name: "Player 1");

        sut.Vip.LoadVipList(friends);

        //acct
        var result = sut.Vip.AddToVip(newFriend);

        //assert
        sut.Vip.VipList.Should().HaveCount(200);
        result.Should().BeFalse();
    }

    [Fact]
    public void Regular_player_cannot_add_special_player()
    {
        //arrange
        var sut = PlayerTestDataBuilder.Build();

        var newFriend = PlayerTestDataBuilder.Build(name: "Player 1");
        newFriend.SetFlag(PlayerFlag.SpecialVip);

        //acct
        var result = sut.Vip.AddToVip(newFriend);

        //assert
        sut.Vip.VipList.Should().HaveCount(0);
        result.Should().BeFalse();
    }

    [Fact]
    public void Special_player_can_add_special_player()
    {
        //arrange
        var sut = PlayerTestDataBuilder.Build();
        sut.SetFlag(PlayerFlag.SpecialVip);

        var newFriend = PlayerTestDataBuilder.Build(name: "Player 1");
        newFriend.SetFlag(PlayerFlag.SpecialVip);

        //acct
        var result = sut.Vip.AddToVip(newFriend);

        //assert
        sut.Vip.VipList.Should().HaveCount(1);
        result.Should().BeTrue();
    }

    [Fact]
    public void Special_player_can_add_regular_player()
    {
        //arrange
        var sut = PlayerTestDataBuilder.Build();
        sut.SetFlag(PlayerFlag.SpecialVip);

        var newFriend = PlayerTestDataBuilder.Build(name: "Player 1");

        //acct
        var result = sut.Vip.AddToVip(newFriend);

        //assert
        sut.Vip.VipList.Should().HaveCount(1);
        result.Should().BeTrue();
    }

    [Fact]
    public void Player_with_empty_name_cannot_be_added_to_vip()
    {
        //arrange
        var sut = PlayerTestDataBuilder.Build();

        var newFriendMock = new Mock<IPlayer>();
        newFriendMock.SetupGet(x => x.Name).Returns(string.Empty);
        //acct
        var result = sut.Vip.AddToVip(newFriendMock.Object);

        //assert
        sut.Vip.VipList.Should().BeEmpty();
        result.Should().BeFalse();
    }
}