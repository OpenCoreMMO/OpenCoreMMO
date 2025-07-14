using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public sealed class PlayerInventoryItemEntity
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int ServerId { get; set; }
    public int SlotId { get; set; }
    public short Amount { get; set; }

    public Dictionary<ItemAttribute, string> Attributes { get; set; } = new();
    public Dictionary<string, string> CustomAttributes { get; set; } = new();

    public PlayerEntity Player { get; set; }
}