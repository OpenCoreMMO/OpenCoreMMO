using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public sealed class PlayerInventoryItemEntity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ServerId { get; set; }
    public int SlotId { get; set; }
    public short Amount { get; set; }
    public ushort? Charges { get; set; }
    public ushort? DecayTo { get; set; }
    public uint? DecayDuration { get; set; }
    public uint? DecayElapsed { get; set; }

    public Dictionary<string, string> Attributes { get; set; } = new();

    public PlayerEntity Player { get; set; }
}