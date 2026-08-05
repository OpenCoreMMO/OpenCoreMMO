using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Data.InMemory.DataStores;

/// <summary>
///     Tracks active house access-list edit windows per player (UI session state).
/// </summary>
public interface IHouseEditWindowStore
{
    uint SetEditHouse(IPlayer player, uint houseId, uint listId);

    bool TryGetCurrent(IPlayer player, out uint windowTextId, out uint houseId, out uint listId);

    bool TryGet(IPlayer player, uint windowTextId, out uint houseId, out uint listId);

    void Clear(IPlayer player);
}
