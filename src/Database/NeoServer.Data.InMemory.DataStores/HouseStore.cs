using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Houses;
using NeoServer.Domain.World.Models.Tiles;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NeoServer.Domain.Tests")]

namespace NeoServer.Data.InMemory.DataStores;

public class HouseStore : DataStore<HouseStore, uint, House>, IHouseStore
{
    public House GetByTile(ITile tile)
    {
        var houseId = (tile as DynamicTile)?.HouseId;
        return houseId.HasValue ? Get(houseId.Value) : null;
    }

    public House GetByHouseId(uint houseId)
    {
        return Get(houseId);
    }
}
