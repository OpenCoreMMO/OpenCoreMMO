using System;

namespace NeoServer.Data.Entities;

public sealed class GuildWarEntity
{
    public int Id { get; set; }
    public int Guild1Id { get; set; }
    public int Guild2Id { get; set; }
    public string Guild1Name { get; set; }
    public string Guild2Name { get; set; }
    public int Status { get; set; } // 0 = ended, 1 = active
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public GuildEntity Guild1 { get; set; }
    public GuildEntity Guild2 { get; set; }
}
