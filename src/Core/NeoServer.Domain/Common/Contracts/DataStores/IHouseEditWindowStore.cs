using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

/// <summary>
///     Tracks active house access-list edit windows per player (UI session state).
///     Contract lives in the domain so consumers depend on the abstraction, not the in-memory implementation.
/// </summary>
public interface IHouseEditWindowStore
{
    uint SetEditHouse(IPlayer player, uint houseId, uint listId);

    bool TryGetCurrent(IPlayer player, out uint windowTextId, out uint houseId, out uint listId);

    bool TryGet(IPlayer player, uint windowTextId, out uint houseId, out uint listId);

    void Clear(IPlayer player);
}
