using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Houses.AccessList;

/// <summary>
///     Ordered access list evaluated top-to-bottom, first-match-wins.
///     Supports player wildcards (<c>*</c>, <c>?</c>), guild names,
///     and guild-rank names. No external DB lookups — all comparisons
///     are string-based at runtime.
/// </summary>
public class HouseAccessList
{
    private readonly List<Entry> _entries = new();

    /// <summary>Gets or sets the raw text representation of this access list.</summary>
    public string RawText { get; set; } = string.Empty; // raw text from persistence, used to detect edits

    /// <summary>Open the house to everyone (matches before any later exclusion).</summary>
    public void AllowEveryone() => _entries.Add(new Entry(EntryType.AllowAll));

    /// <summary>Invite a player matching the pattern. Wildcards <c>*</c> and <c>?</c> supported.</summary>
    public void AddPlayer(string pattern) => _entries.Add(new Entry(EntryType.InvitePlayer, pattern));

    /// <summary>Exclude a player matching the pattern. Wildcards <c>*</c> and <c>?</c> supported.</summary>
    public void AddExcludedPlayer(string pattern) => _entries.Add(new Entry(EntryType.ExcludePlayer, pattern));

    /// <summary>Invite all members of the given guild.</summary>
    public void AddGuild(string guildName) => _entries.Add(new Entry(EntryType.GuildAll, guildName));

    /// <summary>Invite guild members with a specific rank name.</summary>
    public void AddGuildRank(string guildName, string rankName) =>
        _entries.Add(new Entry(EntryType.GuildRank, guildName, rankName));

    /// <summary>Remove all entries.</summary>
    public void Clear() => _entries.Clear();

    /// <summary>
    ///     Check if the player is allowed entry.
    ///     Evaluates entries top-to-bottom; the first matching entry decides.
    /// </summary>
    public bool IsInList(IPlayer player)
    {
        foreach (var entry in _entries)
        {
            switch (entry.Type)
            {
                case EntryType.AllowAll:
                    return true;

                case EntryType.InvitePlayer:
                    if (MatchGlob(player.Name.AsSpan(), entry.Pattern.AsSpan()))
                        return true;
                    break;

                case EntryType.ExcludePlayer:
                    if (MatchGlob(player.Name.AsSpan(), entry.Pattern.AsSpan()))
                        return false;
                    break;

                case EntryType.GuildAll:
                    if (player.HasGuild &&
                        string.Equals(player.Guild?.Name, entry.Pattern, StringComparison.OrdinalIgnoreCase))
                        return true;
                    break;

                case EntryType.GuildRank:
                    if (player.HasGuild &&
                        string.Equals(player.Guild?.Name, entry.Pattern, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(player.GuildRank?.Name, entry.RankName, StringComparison.OrdinalIgnoreCase))
                        return true;
                    break;
            }
        }

        return false;
    }

    /// <summary>
    ///     Glob pattern matcher. Supports <c>*</c> (any sequence) and
    ///     <c>?</c> (single character). Char-by-char, no regex, no allocation.
    /// </summary>
    private static bool MatchGlob(ReadOnlySpan<char> name, ReadOnlySpan<char> pattern)
    {
        int nameIndex = 0, patternIndex = 0;
        int starNameIndex = -1, starPatternIndex = -1;

        while (nameIndex < name.Length)
        {
            if (patternIndex < pattern.Length &&
                (pattern[patternIndex] == '?' ||
                 char.ToLowerInvariant(pattern[patternIndex]) == char.ToLowerInvariant(name[nameIndex])))
            {
                nameIndex++;
                patternIndex++;
            }
            else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                starNameIndex = nameIndex;
                starPatternIndex = patternIndex;
                patternIndex++;
            }
            else if (starPatternIndex >= 0)
            {
                nameIndex = ++starNameIndex;
                patternIndex = starPatternIndex + 1;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            patternIndex++;

        return patternIndex == pattern.Length;
    }

    private readonly record struct Entry(EntryType Type, string Pattern = "", string RankName = "");

    private enum EntryType : byte
    {
        AllowAll,
        InvitePlayer,
        ExcludePlayer,
        GuildAll,
        GuildRank
    }
}
