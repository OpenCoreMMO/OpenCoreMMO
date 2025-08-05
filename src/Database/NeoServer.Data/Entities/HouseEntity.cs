using System;
using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public sealed class HouseEntity
{
    public uint Id { get; set; }
    public uint Owner { get; set; }
    public string Name { get; set; }
    public uint TownId { get; set; }
    public uint Price { get; set; }
    public uint Rent { get; set; }
    public uint Size { get; set; }
    public long PaidUntil { get; set; }
    public DateTime? LastPayment { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    
    // Coordenadas de entrada (formato serializado)
    public string EntryCoordinates { get; set; }
    
    // Tiles da casa (formato serializado)
    public string TileCoordinates { get; set; }
    
    // Portas da casa (formato serializado) 
    public string DoorCoordinates { get; set; }

    public ICollection<HouseListEntity> HouseLists { get; set; }
}