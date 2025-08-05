using System;

namespace NeoServer.Data.Entities;

public sealed class HouseListEntity
{
    public uint HouseId { get; set; }
    public uint PlayerId { get; set; }
    public int ListType { get; set; } // 1=Guest, 2=SubOwner
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public HouseEntity House { get; set; }
}