using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Guild;
using NeoServer.Domain.Houses.AccessList;
using NeoServer.Domain.Tests.Helpers.House;

namespace NeoServer.Domain.Tests.Houses;

public class HouseAccessListTests
{
    [Fact]
    public void Player_name_matched_case_insensitively()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Rookgaard");

        var player = HouseTestDataBuilder.CreatePlayer(name: "ROOKGAARD");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Any_player_allowed_when_allow_all()
    {
        var list = new HouseAccessList();
        list.AllowEveryone();

        var player = HouseTestDataBuilder.CreatePlayer(name: "AnyPlayer");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_member_allowed_by_guild_name()
    {
        var list = new HouseAccessList();
        list.AddGuild("MyGuild");

        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_member_allowed_by_rank_name()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var rank = new GuildRankInfo(1, "Leader", 5);
        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: rank);
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_member_denied_when_rank_name_does_not_match()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var rank = new GuildRankInfo(1, "Member", 3);
        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: rank);
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Guild_member_denied_when_guild_name_does_not_match()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var rank = new GuildRankInfo(1, "Leader", 5);
        var player = HouseTestDataBuilder.CreatePlayer(guildName: "OtherGuild", guildRank: rank);
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void All_entries_cleared()
    {
        var list = new HouseAccessList();
        list.AddPlayer("PlayerA");
        list.AllowEveryone();
        list.Clear();

        var player = HouseTestDataBuilder.CreatePlayer(name: "PlayerA");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Player_denied_when_not_in_list()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Alice");

        var player = HouseTestDataBuilder.CreatePlayer(name: "Bob");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Multiple_players_all_allowed()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Alice");
        list.AddPlayer("Bob");

        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Alice")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Bob")).Should().BeTrue();
    }

    [Fact]
    public void Player_denied_when_exclusion_before_inclusion()
    {
        var list = new HouseAccessList();
        list.AddExcludedPlayer("BadGuy");
        list.AddPlayer("BadGuy");

        var player = HouseTestDataBuilder.CreatePlayer(name: "BadGuy");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Player_allowed_when_inclusion_before_exclusion()
    {
        var list = new HouseAccessList();
        list.AddPlayer("PlayerX");
        list.AddExcludedPlayer("PlayerX");

        var player = HouseTestDataBuilder.CreatePlayer(name: "PlayerX");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Player_matched_by_asterisk_wildcard()
    {
        var list = new HouseAccessList();
        list.AddPlayer("Knight*");

        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "KnightFoo")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Knight")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "SuperKnight")).Should().BeFalse();
    }

    [Fact]
    public void Player_matched_by_question_mark_wildcard()
    {
        var list = new HouseAccessList();
        list.AddPlayer("?ero");

        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Hero")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Zero")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "Heroes")).Should().BeFalse();
    }

    [Fact]
    public void Non_guild_player_denied_by_guild_entry()
    {
        var list = new HouseAccessList();
        list.AddGuild("MyGuild");

        var player = HouseTestDataBuilder.CreatePlayer(name: "SoloPlayer");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Guild_member_allowed_when_guild_name_differs_in_case()
    {
        var list = new HouseAccessList();
        list.AddGuild("MyGuild");

        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MYGUILD");
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_rank_member_allowed_when_guild_name_differs_in_case()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var rank = new GuildRankInfo(1, "Leader", 5);
        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MYGUILD", guildRank: rank);
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_rank_member_allowed_when_rank_name_differs_in_case()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var rank = new GuildRankInfo(1, "LEADER", 5);
        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: rank);
        list.IsInList(player).Should().BeTrue();
    }

    [Fact]
    public void Guild_member_denied_when_excluded_before_guild_entry()
    {
        var list = new HouseAccessList();
        list.AddExcludedPlayer("BadGuy");
        list.AddGuild("MyGuild");

        var member = HouseTestDataBuilder.CreatePlayer(name: "BadGuy", guildName: "MyGuild");
        list.IsInList(member).Should().BeFalse();
    }

    [Fact]
    public void Guild_member_allowed_when_guild_entry_before_player_exclusion()
    {
        var list = new HouseAccessList();
        list.AddGuild("MyGuild");
        list.AddExcludedPlayer("BadGuy");

        var member = HouseTestDataBuilder.CreatePlayer(name: "BadGuy", guildName: "MyGuild");
        list.IsInList(member).Should().BeTrue();
    }

    [Fact]
    public void Members_from_multiple_guilds_all_allowed()
    {
        var list = new HouseAccessList();
        list.AddGuild("GuildA");
        list.AddGuild("GuildB");

        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "GuildA")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "GuildB")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "GuildC")).Should().BeFalse();
    }

    [Fact]
    public void All_players_allowed_when_allow_all_before_exclusion()
    {
        var list = new HouseAccessList();
        list.AllowEveryone();
        list.AddExcludedPlayer("BadGuy");

        // AllowAll comes first → matches all, exclusion after never reached.
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "GoodGuy")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "BadGuy")).Should().BeTrue();
    }

    [Fact]
    public void Excluded_player_denied_when_exclusion_before_allow_all()
    {
        var list = new HouseAccessList();
        list.AddExcludedPlayer("BadGuy");
        list.AllowEveryone();

        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "GoodGuy")).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(name: "BadGuy")).Should().BeFalse();
    }

    [Fact]
    public void Guild_members_with_different_ranks_all_allowed()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Member");
        list.AddGuildRank("MyGuild", "Leader");

        var memberRank = new GuildRankInfo(1, "Member", 1);
        var leaderRank = new GuildRankInfo(2, "Leader", 5);

        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: memberRank)).Should().BeTrue();
        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: leaderRank)).Should().BeTrue();
    }

    [Fact]
    public void Guild_member_with_any_rank_allowed_when_guild_only_entry()
    {
        var list = new HouseAccessList();
        list.AddGuild("MyGuild");

        var anyRank = new GuildRankInfo(1, "Recruit", 1);
        list.IsInList(HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild", guildRank: anyRank)).Should().BeTrue();
    }

    [Fact]
    public void Player_without_guild_denied_for_guild_rank_entry()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var player = HouseTestDataBuilder.CreatePlayer(name: "SoloPlayer");
        list.IsInList(player).Should().BeFalse();
    }

    [Fact]
    public void Guild_member_without_rank_denied_for_guild_rank_entry()
    {
        var list = new HouseAccessList();
        list.AddGuildRank("MyGuild", "Leader");

        var player = HouseTestDataBuilder.CreatePlayer(guildName: "MyGuild");
        list.IsInList(player).Should().BeFalse();
    }
}
