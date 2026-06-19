using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Houses.AccessList;

/// <summary>
/// Who may enter the house. Lists invited players, guilds,
/// guild ranks, or allows everyone. Each house door and access
/// list (guest, sub-owner) has its own instance.
/// </summary>
public class HouseAccessList
{
    private readonly HashSet<string> _playerNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<ushort> _guildIds = new();
    private readonly List<(ushort GuildId, byte RankLevel)> _guildRanks = new();
    private bool _wildcard;

    /// <summary>Let this player enter.</summary>
    public void AddPlayer(string name)
    {
        _playerNames.Add(name);
    }

    /// <summary>Let all members of this guild enter.</summary>
    public void AddGuild(ushort guildId)
    {
        _guildIds.Add(guildId);
    }

    /// <summary>Let guild members at or above this rank enter.</summary>
    public void AddGuildRank(ushort guildId, byte rankLevel)
    {
        _guildRanks.Add((guildId, rankLevel));
    }

    /// <summary>Open the house to everyone.</summary>
    public void AllowAll()
    {
        _wildcard = true;
    }

    /// <summary>Remove all entries. House becomes locked to everyone.</summary>
    public void Clear()
    {
        _playerNames.Clear();
        _guildIds.Clear();
        _guildRanks.Clear();
        _wildcard = false;
    }

    /// <summary>Check if this player is on the access list.</summary>
    public bool IsInList(IPlayer player)
    {
        if (_wildcard) return true;

        if (_playerNames.Contains(player.Name)) return true;

        if (player.HasGuild)
        {
            if (_guildIds.Contains(player.GuildId)) return true;

            if (player.GuildRank is not null)
            {
                return _guildRanks.Any(r => r.GuildId == player.GuildId && player.GuildRank.Level >= r.RankLevel);
            }
        }

        return false;
    }
}