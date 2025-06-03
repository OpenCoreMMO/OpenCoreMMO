namespace NeoServer.Data.Entities;

public sealed class HouseListEntity
{
    public int HouseId { get; set; }
    public int ListId { get; set; }
    public string List { get; set; }
    
    public HouseEntity House { get; set; }
}