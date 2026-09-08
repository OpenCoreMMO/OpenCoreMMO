namespace NeoServer.Data.Entities;

public sealed class HouseTileEntity
{
    public int HouseId { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public int TileZ { get; set; }
    public byte[] Data { get; set; }

    public HouseEntity House { get; set; }
}
