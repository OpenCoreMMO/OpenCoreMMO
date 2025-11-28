using System;

namespace NeoServer.Data.Entities;

public sealed class GuildWarKillEntity
{
    public int Id { get; set; }
    public string Killer { get; set; }
    public string Target { get; set; }
    public int KillerGuildId { get; set; }
    public int TargetGuildId { get; set; }
    public int WarId { get; set; }
    public DateTime Timestamp { get; set; }

    public GuildEntity KillerGuild { get; set; }
    public GuildEntity TargetGuild { get; set; }
    public GuildWarEntity War { get; set; }
}