using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Contracts.World.Tiles;

public interface ITileFactory
{
    ITile CreateTile(Coordinate coordinate, TileFlag flag, IItem[] items, bool useCache = true, uint? houseId = null);
    ITile CreateDynamicTile(Coordinate coordinate, TileFlag flag, IItem[] items);
    ITile GetTileFromCache(Coordinate coordinate, ref Span<byte> clientIds);
}