using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseAccessListTests
{
    [Fact]
    public void AddPlayer_AddsNameCaseInsensitive()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Rookgaard");

        var player = HouseTestDataBuilder.CreatePlayer(name: "ROOKGAARD");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void AllowAll_MakesAnyPlayerInList()
    {
        var list = new HouseAccessList();
        list.AllowAll();

        var player = HouseTestDataBuilder.CreatePlayer(name: "AnyPlayer");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void AddGuild_AddsGuildId()
    {
        var list = new HouseAccessList();
        list.AddGuild(100);

        var player = HouseTestDataBuilder.CreatePlayer(guildId: 100);
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void AddGuildRank_MatchesRankLevelOrAbove()
    {
        var list = new HouseAccessList();
        list.AddGuildRank(100, 3);

        var rank = new GuildRankInfo(1, "Leader", 5);
        var player = HouseTestDataBuilder.CreatePlayer(guildId: 100, guildRank: rank);
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void AddGuildRank_LowerRankDoesNotMatch()
    {
        var list = new HouseAccessList();
        list.AddGuildRank(100, 5);

        var rank = new GuildRankInfo(1, "Member", 3);
        var player = HouseTestDataBuilder.CreatePlayer(guildId: 100, guildRank: rank);
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Clear_ResetsAllEntries()
    {
        var list = new HouseAccessList();
        list.AddPlayer("PlayerA");
        list.AllowAll();
        list.Clear();

        var player = HouseTestDataBuilder.CreatePlayer(name: "PlayerA");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void IsInList_PlayerNotInList_ReturnsFalse()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Alice");

        var player = HouseTestDataBuilder.CreatePlayer(name: "Bob");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void AddPlayer_MultipleNames_AllMatch()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Alice");
        list.AddPlayer("Bob");

        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Alice")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Bob")).Should().BeTrue();
    }
}
