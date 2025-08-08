using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Guild;

public class Guild : IBankable
{
    private readonly List<IPlayer> _membersOnline = new();
    private readonly List<IPlayer> _members = new();
    private readonly List<IPlayer> _invitedPlayers = new();
    private readonly Dictionary<ushort, GuildRankInfo> _ranks = new();
    private readonly List<ushort> _guildWarList = new();

    public ushort Id { get; init; }
    public string Name { get; set; }
    public IDictionary<ushort, GuildLevel> GuildLevels { get; set; }
    public ChatChannel Channel { get; set; }
    public string Motd { get; set; } = string.Empty;
    public uint MemberCount { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public ushort OwnerId { get; set; }
    public IPlayer Leader { get; private set; }

    public required IBank Bank { get; init; }
    public ulong BankAmount => Bank?.Amount ?? 0;

    public IReadOnlyList<IPlayer> MembersOnline => _membersOnline.AsReadOnly();
    public IReadOnlyList<IPlayer> Members => _members.AsReadOnly();
    public IReadOnlyDictionary<ushort, GuildRankInfo> Ranks => _ranks.AsReadOnly();
    public IReadOnlyList<ushort> GuildWarList => _guildWarList.AsReadOnly();

    public bool HasMember(IPlayer player)
    {
        return player.GuildId == Id;
    }

    public GuildLevel GetMemberLevel(IPlayer player)
    {
        return GuildLevels is null ? null : GuildLevels.TryGetValue(player.Level, out var level) ? level : null;
    }

    public string InspectionText(IPlayer player)
    {
        if (player.GuildRank == null)
            return $"{player.GenderPronoun} is member of the {Name}.";

        var rankName = player.GuildRank.Name;
        return $"{player.GenderPronoun} is {rankName} of the {Name}.";
    }

    public void AddMember(IPlayer player)
    {
        if (!_membersOnline.Contains(player))
        {
            _membersOnline.Add(player);
            
            // Also add to general members list if not already there
            if (!_members.Contains(player))
            {
                _members.Add(player);
                player.SetGuild(this);
            }
            
            // Remove from invitations if they were invited
            _invitedPlayers.Remove(player);
            
            // Update player helpers for all online members
            UpdateMemberHelpers();
        }
    }

    public void RemoveMember(IPlayer player)
    {
        if (_membersOnline.Remove(player))
        {
            _members.Remove(player);
            player.SetGuild(null);
            
            // Update player helpers for all remaining online members
            UpdateMemberHelpers();
        }
    }

    public GuildRankInfo GetRankById(ushort rankId)
    {
        _ranks.TryGetValue(rankId, out var rank);
        return rank;
    }

    public GuildRankInfo GetRankByLevel(byte level)
    {
        return _ranks.Values.FirstOrDefault(r => r.Level == level);
    }

    public GuildRankInfo GetRankByName(string name)
    {
        return _ranks.Values.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void AddRank(ushort rankId, string rankName, byte level)
    {
        var rank = new GuildRankInfo
        {
            Id = rankId,
            Name = rankName,
            Level = level
        };
        _ranks[rankId] = rank;
    }

    public bool IsGuildMate(IPlayer otherPlayer)
    {
        return otherPlayer?.GuildId == Id;
    }

    public bool IsInWar(IPlayer otherPlayer)
    {
        if (otherPlayer?.GuildId == 0) return false;
        return _guildWarList.Contains(otherPlayer.GuildId) && 
               IsPlayerInWarList(otherPlayer, Id);
    }

    public void AddWarGuild(ushort guildId)
    {
        if (!_guildWarList.Contains(guildId))
        {
            _guildWarList.Add(guildId);
        }
    }

    public void RemoveWarGuild(ushort guildId)
    {
        _guildWarList.Remove(guildId);
    }

    public void SetLeader(IPlayer player)
    {
        if (player == null) return;
        
        Leader = player;
        OwnerId = (ushort)player.Id;
        
        // Add player to guild if not already a member
        if (!HasMember(player))
        {
            AddMember(player);
            _members.Add(player);
        }
        
        // Set player's guild
        player.SetGuild(this);
        
        // Set player's guild rank to Leader (level 3)
        var leaderRank = GetRankByLevel(3); // Leader rank
        
        if (leaderRank != null)
        {
            player.GuildRank = leaderRank;
        }
    }

    public bool InvitePlayer(IPlayer player)
    {
        if (player == null || HasMember(player) || _invitedPlayers.Contains(player))
            return false;
            
        _invitedPlayers.Add(player);
        return true;
    }

    public bool HasInvitation(IPlayer player)
    {
        return player != null && _invitedPlayers.Contains(player);
    }

    public bool RevokeInvitation(IPlayer player)
    {
        return player != null && _invitedPlayers.Remove(player);
    }

    public bool PromoteMember(IPlayer player, int newLevel)
    {
        if (player == null || !HasMember(player) || newLevel < 1 || newLevel > 3)
            return false;
            
        // TODO: Set guild level through proper domain method when available
        return true;
    }

    public bool DemoteMember(IPlayer player, int newLevel)
    {
        if (player == null || !HasMember(player) || newLevel < 1 || newLevel > 3)
            return false;
            
        // TODO: Set guild level through proper domain method when available
        return true;
    }

    public bool Disband()
    {
        // Remove all members
        foreach (var member in _members.ToList())
        {
            member.SetGuild(null);
        }
        
        foreach (var memberOnline in _membersOnline.ToList())
        {
            memberOnline.SetGuild(null);
        }
        
        _members.Clear();
        _membersOnline.Clear();
        _invitedPlayers.Clear();
        
        return true;
    }

    private void UpdateMemberHelpers()
    {
        // TODO: Implement player helper updates when available in the domain
        // This would update visibility/helpers for all guild members
    }

    private static bool IsPlayerInWarList(IPlayer player, ushort guildId)
    {
        // TODO: This should check if the player's guild war list contains this guild ID
        // For now, we assume the reciprocal relationship exists
        return true;
    }
}

public class GuildLevel : IEquatable<GuildLevel>
{
    private string levelName;

    public GuildLevel(GuildRank level = GuildRank.Member, string levelName = null)
    {
        Level = level;
        LevelName = levelName;
    }

    public ushort Id { get; init; }

    public string LevelName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(levelName)) return levelName;
            return Level switch
            {
                GuildRank.Leader => "Leader",
                GuildRank.ViceLeader => "Vice-Leader",
                _ => "Member"
            };
        }
        private set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            levelName = value;
        }
    }

    public GuildRank Level { get; }

    public bool Equals(GuildLevel other)
    {
        return other.Id == Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}