using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Houses;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IHouseStore : IDataStore<uint, House>
{
    House GetByTile(ITile tile);
    House GetByHouseId(uint houseId);
}
