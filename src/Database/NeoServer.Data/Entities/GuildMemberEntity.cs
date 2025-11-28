using System;

namespace NeoServer.Data.Entities;

public sealed class GuildMemberEntity
{
    public int PlayerId { get; set; }
    public int GuildId { get; set; }
    public ushort RankId { get; set; }
    public string Nick { get; set; }
    public string Rank { get; set; }
    public DateTime JoinedAt { get; set; }

    public PlayerEntity Player { get; set; }
    public GuildEntity Guild { get; set; }
}