using System.Linq;
using FluentAssertions;
using NeoServer.Loaders.Houses;
using Xunit;

namespace NeoServer.Loaders.Tests.Houses;

public class HouseAccessListParserTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_null_or_whitespace_returns_empty_result(string text)
    {
        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Parse_comment_lines_are_skipped()
    {
        var text = "# this is a comment\n#another one\nRook";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().ContainSingle();
        result.Entries[0].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[0].Pattern.Should().Be("Rook");
    }

    [Fact]
    public void Parse_exclusion_entry()
    {
        var text = "!BadGuy\nGoodGuy";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Kind.Should().Be(ListEntryKind.ExcludePlayer);
        result.Entries[0].Pattern.Should().Be("BadGuy");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("GoodGuy");
    }

    [Fact]
    public void Parse_exclusion_with_wildcard()
    {
        var text = "!Troll*\nTrollKing\nTrollMaster\nHero";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(4);
        result.Entries[0].Kind.Should().Be(ListEntryKind.ExcludePlayer);
        result.Entries[0].Pattern.Should().Be("Troll*");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("TrollKing");
        result.Entries[2].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[2].Pattern.Should().Be("TrollMaster");
        result.Entries[3].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[3].Pattern.Should().Be("Hero");
    }

    [Fact]
    public void Parse_allow_all()
    {
        var text = "*";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().ContainSingle();
        result.Entries[0].Kind.Should().Be(ListEntryKind.AllowAll);
        result.Entries[0].Pattern.Should().Be("*");
    }

    [Fact]
    public void Parse_allow_all_with_exclusion_after_is_skipped_by_loader()
    {
        // According to Tibia docs: if * comes first, a later exclusion
        // has no effect because the player was already invited.
        var text = "*\n!BadGuy";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Kind.Should().Be(ListEntryKind.AllowAll);
        result.Entries[1].Kind.Should().Be(ListEntryKind.ExcludePlayer);
        result.Entries[1].Pattern.Should().Be("BadGuy");
    }

    [Fact]
    public void Parse_exact_player_names()
    {
        var text = "Rookgaard\nDragon Slayer";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Pattern.Should().Be("Rookgaard");
        result.Entries[1].Pattern.Should().Be("Dragon Slayer");
        result.Entries.All(e => e.Kind == ListEntryKind.InvitePlayer).Should().BeTrue();
    }

    [Fact]
    public void Parse_player_name_with_wildcard_preserves_pattern()
    {
        var text = "Knight*\nA?ocalypse\nOr?ha?a?l\nA*a\nA*b*a";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(5);
        result.Entries[0].Pattern.Should().Be("Knight*");
        result.Entries[1].Pattern.Should().Be("A?ocalypse");
        result.Entries[2].Pattern.Should().Be("Or?ha?a?l");
        result.Entries[3].Pattern.Should().Be("A*a");
        result.Entries[4].Pattern.Should().Be("A*b*a");
        result.Entries.All(e => e.Kind == ListEntryKind.InvitePlayer).Should().BeTrue();
    }

    [Fact]
    public void Parse_guild_reference_all_members()
    {
        var text = "*@MyGuild";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().ContainSingle();
        result.Entries[0].Kind.Should().Be(ListEntryKind.GuildAll);
        result.Entries[0].Pattern.Should().Be("MyGuild");
        result.Entries[0].Rank.Should().Be("*");
    }

    [Fact]
    public void Parse_guild_rank_reference()
    {
        var text = "Leader@MyGuild";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().ContainSingle();
        result.Entries[0].Kind.Should().Be(ListEntryKind.GuildRank);
        result.Entries[0].Pattern.Should().Be("MyGuild");
        result.Entries[0].Rank.Should().Be("Leader");
    }

    [Fact]
    public void Parse_preserves_entry_order()
    {
        var text = "# comment\n!ExcludedGuy\nAllowedGuy\n*@AwesomeGuild\nMember@OtherGuild\n*";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(5);
        result.Entries[0].Kind.Should().Be(ListEntryKind.ExcludePlayer);
        result.Entries[0].Pattern.Should().Be("ExcludedGuy");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("AllowedGuy");
        result.Entries[2].Kind.Should().Be(ListEntryKind.GuildAll);
        result.Entries[2].Pattern.Should().Be("AwesomeGuild");
        result.Entries[3].Kind.Should().Be(ListEntryKind.GuildRank);
        result.Entries[3].Pattern.Should().Be("OtherGuild");
        result.Entries[3].Rank.Should().Be("Member");
        result.Entries[4].Kind.Should().Be(ListEntryKind.AllowAll);
    }

    [Fact]
    public void Parse_blank_lines_are_skipped()
    {
        var text = "Player1\n\n\nPlayer2";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[0].Pattern.Should().Be("Player1");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("Player2");
    }

    [Fact]
    public void Parse_trailing_whitespace_is_trimmed()
    {
        var text = "  Player1  \n  Player2  ";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[0].Pattern.Should().Be("Player1");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("Player2");
    }

    [Fact]
    public void Parse_hash_at_start_is_comment_anywhere_else_is_player()
    {
        var text = "# comment\nPlayer #1";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(1);
        result.Entries[0].Pattern.Should().Be("Player #1");
    }

    [Fact]
    public void Parse_exclamation_at_start_excludes_anywhere_else_allows()
    {
        var text = "!BadGuy\nWow! Player";

        var result = HouseAccessListParser.Parse(text);

        result.Entries.Should().HaveCount(2);
        result.Entries[0].Kind.Should().Be(ListEntryKind.ExcludePlayer);
        result.Entries[0].Pattern.Should().Be("BadGuy");
        result.Entries[1].Kind.Should().Be(ListEntryKind.InvitePlayer);
        result.Entries[1].Pattern.Should().Be("Wow! Player");
    }
}
