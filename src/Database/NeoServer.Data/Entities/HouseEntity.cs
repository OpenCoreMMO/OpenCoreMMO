using System.Collections.Generic;

namespace NeoServer.Data.Entities;

public sealed class HouseEntity
{
    public int Id { get; set; }
    public int Onwer { get; set; }
    public int Paid { get; set; }
    public int Warnings { get; set; }
    public string Name { get; set; }
    public int Rent { get; set; }
    public int TownId { get; set; }
    public int Bid { get; set; }
    public int BidEnd { get; set; }
    public int LastBid { get; set; }
    public int HighestBidder { get; set; }
    public int Size { get; set; }
    public int Beds { get; set; }
    
    public ICollection<HouseListEntity> HouseLists { get; set; }
}