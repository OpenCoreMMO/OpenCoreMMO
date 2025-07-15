using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public abstract class PlayerItemBaseEntity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public short Amount { get; set; }
    public int ParentId { get; set; }
    public int ServerId { get; set; }

    public ushort? DecayTo { get; set; }
    public uint? DecayDuration { get; set; }
    public uint? DecayElapsed { get; set; }

    public ushort? Charges { get; set; }
    public int ContainerId { get; set; } = 0;

    public Dictionary<string, string> Attributes { get; set; } = new();

    public virtual PlayerEntity Player { get; set; }
}