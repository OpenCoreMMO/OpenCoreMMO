using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Houses;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NeoServer.Domain.Tests")]

namespace NeoServer.Data.InMemory.DataStores;

public class HouseStore : DataStore<HouseStore, uint, House>, IHouseStore
{
    public House GetByTile(ITile tile)
    {
        if (tile is not IDynamicTile dynamicTile || dynamicTile.HouseId is not > 0)
            return null;

        return Get(dynamicTile.HouseId.Value);
    }

    public House GetByHouseId(uint houseId)
    {
        return Get(houseId);
    }

    public House GetByOwnerGuid(uint ownerGuid)
    {
        if (ownerGuid == 0)
        {
            return null;
        }

        foreach (var house in All)
        {
            if (house.OwnerGuid == ownerGuid)
                return house;
        }

        return null;
    }
}
