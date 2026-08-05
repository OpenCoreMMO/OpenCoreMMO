using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

/// <summary>
///     Tracks the active house access-list edit session per player.
///     Keeps UI edit state out of the domain Player.
/// </summary>
public class HouseEditWindowStore : DataStore<HouseEditWindowStore, uint, HouseEditWindowSession>, IHouseEditWindowStore
{
    public uint SetEditHouse(IPlayer player, uint houseId, uint listId)
    {
        var playerId = player.Id;
        var windowTextId = 1u;

        if (TryGetValue(playerId, out var session))
        {
            windowTextId = session.WindowTextId + 1;
        }

        var updated = new HouseEditWindowSession(windowTextId, houseId, listId);
        AddOrUpdate(playerId, updated);
        return windowTextId;
    }

    public bool TryGetCurrent(IPlayer player, out uint windowTextId, out uint houseId, out uint listId)
    {
        windowTextId = 0;
        houseId = 0;
        listId = 0;

        if (!TryGetValue(player.Id, out var session) || session.WindowTextId == 0)
        {
            return false;
        }

        windowTextId = session.WindowTextId;
        houseId = session.HouseId;
        listId = session.ListId;
        return true;
    }

    public bool TryGet(IPlayer player, uint windowTextId, out uint houseId, out uint listId)
    {
        houseId = 0;
        listId = 0;

        if (!TryGetValue(player.Id, out var session))
        {
            return false;
        }

        if (session.WindowTextId != windowTextId)
        {
            return false;
        }

        houseId = session.HouseId;
        listId = session.ListId;
        return true;
    }

    public void Clear(IPlayer player)
    {
        Map.Remove(player.Id);
    }
}
