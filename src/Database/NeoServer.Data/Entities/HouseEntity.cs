using System;
using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public sealed class HouseEntity
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public DateTime? PaidUntil { get; set; }
    public string OwnerName { get; set; }
    public int OwnerAccountId { get; set; }
    public int? EntryX { get; set; }
    public int? EntryY { get; set; }
    public int? EntryZ { get; set; }
    public int Warnings { get; set; }
    public string Name { get; set; }
    public int Rent { get; set; }
    public int TownId { get; set; }
    public int Size { get; set; }
    public int Beds { get; set; }

    public ICollection<HouseListEntity> HouseLists { get; set; }
}