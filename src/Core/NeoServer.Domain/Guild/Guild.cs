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
        Console.WriteLine($"[DEBUG] AddMember called for player: {player?.Name ?? "null"}");
        
        if (player == null)
        {
            Console.WriteLine("[DEBUG] AddMember failed - Player is null");
            return;
        }
        
        Console.WriteLine($"[DEBUG] Player ID: {player.Id}, Guild ID: {Id}");
        Console.WriteLine($"[DEBUG] Members online count before: {_membersOnline.Count}");
        Console.WriteLine($"[DEBUG] Members total count before: {_members.Count}");
        Console.WriteLine($"[DEBUG] Is player already in online list: {_membersOnline.Contains(player)}");
        Console.WriteLine($"[DEBUG] Is player already in members list: {_members.Contains(player)}");
        
        if (!_membersOnline.Contains(player))
        {
            _membersOnline.Add(player);
            Console.WriteLine($"[DEBUG] Player {player.Name} added to online members list");
            
            // Also add to general members list if not already there
            if (!_members.Contains(player))
            {
                _members.Add(player);
                Console.WriteLine($"[DEBUG] Player {player.Name} added to all members list");
                player.SetGuild(this);
                Console.WriteLine($"[DEBUG] Player guild set via SetGuild - Player GuildId: {player.GuildId}");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Player {player.Name} already in all members list");
            }
            
            // Remove from invitations if they were invited
            if (_invitedPlayers.Remove(player))
            {
                Console.WriteLine($"[DEBUG] Player {player.Name} removed from invitations list");
            }
            
            // Update player helpers for all online members
            UpdateMemberHelpers();
            
            Console.WriteLine($"[DEBUG] Members online count after: {_membersOnline.Count}");
            Console.WriteLine($"[DEBUG] Members total count after: {_members.Count}");
        }
        else
        {
            Console.WriteLine($"[DEBUG] Player {player.Name} already in online members list");
        }
    }

    public bool RemoveMember(IPlayer player)
    {
        Console.WriteLine($"[DEBUG] RemoveMember called for player: {player?.Name ?? "null"}");
        
        if (player == null)
        {
            Console.WriteLine("[DEBUG] RemoveMember failed - Player is null");
            return false;
        }
        
        Console.WriteLine($"[DEBUG] Player guild ID: {player.GuildId}, This guild ID: {Id}");
        Console.WriteLine($"[DEBUG] Members online count before: {_membersOnline.Count}");
        Console.WriteLine($"[DEBUG] Members total count before: {_members.Count}");
        Console.WriteLine($"[DEBUG] Online members: {string.Join(", ", _membersOnline.Select(m => $"{m.Name}(ID:{m.Id})"))}");
        Console.WriteLine($"[DEBUG] All members: {string.Join(", ", _members.Select(m => $"{m.Name}(ID:{m.Id})"))}");
        Console.WriteLine($"[DEBUG] Target player ID: {player.Id}");
        Console.WriteLine($"[DEBUG] Is player in online list: {_membersOnline.Contains(player)}");
        Console.WriteLine($"[DEBUG] Is player in members list: {_members.Contains(player)}");
        
        bool wasRemoved = false;
        
        // Remove from online members list
        if (_membersOnline.Remove(player))
        {
            Console.WriteLine("[DEBUG] Player removed from online members list");
            wasRemoved = true;
        }
        
        // Remove from all members list
        if (_members.Remove(player))
        {
            Console.WriteLine("[DEBUG] Player removed from all members list");
            wasRemoved = true;
        }
        
        if (wasRemoved)
        {
            // Force exit from guild channel BEFORE removing guild membership
            // This ensures the guild chat window is properly closed on the client
            if (Channel != null && player.Channels.PrivateChannels?.Contains(Channel) == true)
            {
                Console.WriteLine("[DEBUG] Forcing player to exit guild channel");
                player.Channels.ExitChannel(Channel);
            }
            
            Console.WriteLine("[DEBUG] Setting player guild to null");
            player.SetGuild(null);
            Console.WriteLine($"[DEBUG] Player guild ID after SetGuild(null): {player.GuildId}");
            
            // Update player helpers for all remaining online members
            UpdateMemberHelpers();
            
            Console.WriteLine($"[DEBUG] Members online count after: {_membersOnline.Count}");
            Console.WriteLine($"[DEBUG] Members total count after: {_members.Count}");
            Console.WriteLine("[DEBUG] RemoveMember succeeded");
        }
        else
        {
            Console.WriteLine("[DEBUG] RemoveMember failed - Player was not in any member list");
        }
        
        return wasRemoved;
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
        Console.WriteLine($"[DEBUG] SetLeader called for player: {player?.Name ?? "null"}");
        if (player == null) 
        {
            Console.WriteLine("[DEBUG] SetLeader failed - Player is null");
            return;
        }
        
        Leader = player;
        OwnerId = (ushort)player.Id;
        Console.WriteLine($"[DEBUG] Leader and OwnerId set - Leader: {Leader.Name}, OwnerId: {OwnerId}");
        
        // Add player to guild if not already a member
        if (!HasMember(player))
        {
            Console.WriteLine("[DEBUG] Player not a member, adding to guild");
            AddMember(player);
            _members.Add(player);
        }
        else
        {
            Console.WriteLine("[DEBUG] Player already a member of guild");
        }
        
        // Set player's guild
        player.SetGuild(this);
        Console.WriteLine($"[DEBUG] Player guild set - GuildId: {player.GuildId}");
        
        // Set player's guild rank to Leader (level 3)
        var leaderRank = GetRankByLevel(3); // Leader rank
        Console.WriteLine($"[DEBUG] Looking for leader rank (level 3)");
        Console.WriteLine($"[DEBUG] Available ranks: {string.Join(", ", _ranks.Values.Select(r => $"{r.Name}(L{r.Level})"))}");
        
        if (leaderRank != null)
        {
            player.GuildRank = leaderRank;
            Console.WriteLine($"[DEBUG] Leader rank set successfully - Rank: {leaderRank.Name} (Level: {leaderRank.Level})");
        }
        else
        {
            Console.WriteLine("[DEBUG] SetLeader failed - Could not find leader rank!");
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
        Console.WriteLine($"[DEBUG] PromoteMember called - Player: {player?.Name ?? "null"}, NewLevel: {newLevel}");
        
        if (player == null)
        {
            Console.WriteLine("[DEBUG] PromoteMember failed - Player is null");
            return false;
        }
        
        Console.WriteLine($"[DEBUG] Player GuildId: {player.GuildId}, Guild Id: {Id}");
        if (!HasMember(player))
        {
            Console.WriteLine("[DEBUG] PromoteMember failed - Player is not a guild member");
            return false;
        }
        
        if (newLevel < 1 || newLevel > 3)
        {
            Console.WriteLine($"[DEBUG] PromoteMember failed - Invalid level: {newLevel}");
            return false;
        }
            
        // Cannot promote to leader level (that's transfer leadership)
        if (newLevel == 3)
        {
            Console.WriteLine("[DEBUG] PromoteMember failed - Cannot promote to leader level");
            return false;
        }
        
        Console.WriteLine($"[DEBUG] Current player rank: {player.GuildRank?.Name ?? "null"} (Level: {player.GuildRank?.Level ?? 0})");
        
        // Get the new rank
        var newRank = GetRankByLevel((byte)newLevel);
        if (newRank == null)
        {
            Console.WriteLine($"[DEBUG] PromoteMember failed - Could not find rank for level {newLevel}");
            Console.WriteLine($"[DEBUG] Available ranks: {string.Join(", ", _ranks.Values.Select(r => $"{r.Name}(L{r.Level})"))}");
            return false;
        }
        
        Console.WriteLine($"[DEBUG] Found new rank: {newRank.Name} (Level: {newRank.Level})");
        
        // Update player's guild rank
        player.GuildRank = newRank;
        Console.WriteLine($"[DEBUG] PromoteMember succeeded - Player promoted to {newRank.Name}");
        return true;
    }

    public bool DemoteMember(IPlayer player, int newLevel)
    {
        if (player == null || !HasMember(player) || newLevel < 1 || newLevel > 3)
            return false;
            
        // Cannot demote the leader
        if (player.GuildRank?.Level == 3)
            return false;
            
        // Get the new rank
        var newRank = GetRankByLevel((byte)newLevel);
        if (newRank == null)
            return false;
            
        // Update player's guild rank
        player.GuildRank = newRank;
        return true;
    }

    public bool TransferLeadership(IPlayer currentLeader, IPlayer newLeader)
    {
        Console.WriteLine($"[DEBUG] TransferLeadership called - Current: {currentLeader?.Name ?? "null"}, New: {newLeader?.Name ?? "null"}");
        
        if (currentLeader == null || newLeader == null)
        {
            Console.WriteLine("[DEBUG] TransferLeadership failed - One of the players is null");
            return false;
        }
        
        if (!HasMember(currentLeader) || !HasMember(newLeader))
        {
            Console.WriteLine("[DEBUG] TransferLeadership failed - One of the players is not a guild member");
            return false;
        }
        
        if (currentLeader.GuildRank?.Level != 3)
        {
            Console.WriteLine("[DEBUG] TransferLeadership failed - Current player is not the leader");
            return false;
        }
        
        if (newLeader.GuildRank?.Level < 1)
        {
            Console.WriteLine("[DEBUG] TransferLeadership failed - Target player is not a valid member");
            return false;
        }
        
        // Get ranks
        var leaderRank = GetRankByLevel(3);
        var viceLeaderRank = GetRankByLevel(2);
        
        if (leaderRank == null || viceLeaderRank == null)
        {
            Console.WriteLine("[DEBUG] TransferLeadership failed - Could not find required ranks");
            return false;
        }
        
        // Transfer leadership
        currentLeader.GuildRank = viceLeaderRank;
        newLeader.GuildRank = leaderRank;
        
        Console.WriteLine($"[DEBUG] TransferLeadership succeeded - {newLeader.Name} is now leader, {currentLeader.Name} is now vice-leader");
        return true;
    }

    public bool Disband()
    {
        Console.WriteLine($"[DEBUG] Disband called for guild: {Name} (ID: {Id})");
        
        try
        {
            // Remove all members from guild first
            Console.WriteLine($"[DEBUG] Removing {_members.Count} regular members and {_membersOnline.Count} online members");
            
            // Remove all online members first (includes channel exit)
            foreach (var memberOnline in _membersOnline.ToList())
            {
                Console.WriteLine($"[DEBUG] Removing online member: {memberOnline.Name}");
                var removed = RemoveMember(memberOnline); // This handles channel exit and guild removal
                Console.WriteLine($"[DEBUG] Member {memberOnline.Name} removal result: {removed}");
            }
            
            // Remove any remaining offline members
            foreach (var member in _members.ToList())
            {
                Console.WriteLine($"[DEBUG] Removing offline member: {member.Name}");
                member.SetGuild(null);
                _members.Remove(member);
            }
            
            // Clear all collections
            _members.Clear();
            _membersOnline.Clear();
            _invitedPlayers.Clear();
            _ranks.Clear();
            
            // Close guild channel if it exists
            if (Channel != null)
            {
                Console.WriteLine($"[DEBUG] Closing guild channel: {Channel.Id}");
                // Remove all users from the channel
                foreach (var user in Channel.Users.ToList())
                {
                    // Find the player and exit them from the channel
                    var channelPlayer = user.Player;
                    if (channelPlayer != null)
                    {
                        Console.WriteLine($"[DEBUG] Exiting player {channelPlayer.Name} from guild channel");
                        channelPlayer.Channels.ExitChannel(Channel);
                    }
                }
                Channel = null;
            }
            
            // Clear leader reference
            Leader = null;
            OwnerId = 0;
            
            Console.WriteLine("[DEBUG] Guild disbanded successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Error disbanding guild: {ex.Message}");
            return false;
        }
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