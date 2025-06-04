using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Guilds;

namespace NeoServer.Domain.Creatures.Guild;

public class Guild: IBankable
{
    public ushort Id { get; init; }
    public string Name { get; set; }
    public IDictionary<ushort, GuildLevel> GuildLevels { get; set; }
    public ChatChannel Channel { get; set; }

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
        return $"{player.GenderPronoun} is member of the {Name}.";
    }

    public required IBank Bank { get; init; }
    public ulong BankAmount => Bank?.Amount ?? 0;
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

    public bool Equals(GuildLevel other)
    {
        return other.Id == Id;
    }

    public GuildRank Level { get; }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}