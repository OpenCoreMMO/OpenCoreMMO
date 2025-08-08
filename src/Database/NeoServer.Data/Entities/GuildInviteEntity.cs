using System;

namespace NeoServer.Data.Entities;

public sealed class GuildInviteEntity
{
    public int PlayerId { get; set; }
    public int GuildId { get; set; }
    public DateTime InvitedAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }

    public PlayerEntity Player { get; set; }
    public GuildEntity Guild { get; set; }
}
