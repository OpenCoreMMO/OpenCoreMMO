using System;
using System.Collections.Generic;

namespace NeoServer.Loaders.Houses;

/// <summary>
///     Pure text parser for house-list grammar. No external dependencies.
///     Parses top-to-bottom, preserving entry order.
/// </summary>
public static class HouseAccessListParser
{
    public static ParseResult Parse(string text)
    {
        var entries = new List<ListEntry>();

        if (string.IsNullOrWhiteSpace(text)) return new ParseResult(entries);

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Comment — must start line
            if (trimmed[0] == '#') continue;

            // Exclusion — must start line
            if (trimmed[0] == '!')
            {
                var pattern = trimmed[1..].Trim();
                if (!string.IsNullOrEmpty(pattern))
                    entries.Add(new ListEntry(ListEntryKind.ExcludePlayer, pattern));
                continue;
            }

            // Allow all
            if (trimmed == "*")
            {
                entries.Add(new ListEntry(ListEntryKind.AllowAll, "*"));
                continue;
            }

            // Guild reference: *@guildname or rankname@guildname
            if (TryParseGuildAtLine(trimmed, out var rank, out var guildName))
            {
                var kind = rank == "*" ? ListEntryKind.GuildAll : ListEntryKind.GuildRank;
                entries.Add(new ListEntry(kind, guildName, rank));
                continue;
            }

            // Anything else is a player entry (wildcards * ? allowed anywhere)
            entries.Add(new ListEntry(ListEntryKind.InvitePlayer, trimmed));
        }

        return new ParseResult(entries);
    }

    private static bool TryParseGuildAtLine(string line, out string rank, out string guildName)
    {
        rank = null;
        guildName = null;

        var atIndex = line.IndexOf('@');
        if (atIndex <= 0) return false;

        rank = line[..atIndex].Trim();
        guildName = line[(atIndex + 1)..].Trim();

        return !string.IsNullOrEmpty(rank) && !string.IsNullOrEmpty(guildName);
    }
}

public readonly record struct ParseResult(IReadOnlyList<ListEntry> Entries);

public readonly record struct ListEntry(ListEntryKind Kind, string Pattern, string Rank = "");

public enum ListEntryKind
{
    /// <summary>Invite all players.</summary>
    AllowAll,
    /// <summary>Invite a player matching the pattern (wildcards: * ? ).</summary>
    InvitePlayer,
    /// <summary>Exclude a player matching the pattern.</summary>
    ExcludePlayer,
    /// <summary>Invite all guild members.</summary>
    GuildAll,
    /// <summary>Invite guild members of a specific rank.</summary>
    GuildRank
}
