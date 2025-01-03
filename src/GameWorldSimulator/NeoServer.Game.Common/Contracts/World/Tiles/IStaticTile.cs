namespace NeoServer.Game.Common.Contracts.World.Tiles;

public interface IStaticTile : ITile
{
    byte[] Raw { get; }
    ushort[] AllClientIdItems { get; }

    /// <summary>
    /// Create new instance of StaticTile setting new location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    IStaticTile CreateClone(Location.Structs.Location location);
}