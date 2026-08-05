using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Data.InMemory.DataStores;

/// <summary>
///     Tracks the active house access-list edit session per player (window id, house, list).
///     Keeps UI edit state out of the domain Player.
/// </summary>
public static class HouseEditWindowStore
{
    private static readonly Dictionary<uint, EditSession> Sessions = new();

    public static uint SetEditHouse(IPlayer player, uint houseId, uint listId)
    {
        if (!Sessions.TryGetValue(player.Id, out var session))
        {
            session = new EditSession();
            Sessions[player.Id] = session;
        }

        session.WindowTextId++;
        session.HouseId = houseId;
        session.ListId = listId;
        return session.WindowTextId;
    }

    public static bool TryGetCurrent(IPlayer player, out uint windowTextId, out uint houseId, out uint listId)
    {
        windowTextId = 0;
        houseId = 0;
        listId = 0;

        if (!Sessions.TryGetValue(player.Id, out var session) || session.WindowTextId == 0)
        {
            return false;
        }

        windowTextId = session.WindowTextId;
        houseId = session.HouseId;
        listId = session.ListId;
        return true;
    }

    public static bool TryGet(IPlayer player, uint windowTextId, out uint houseId, out uint listId)
    {
        houseId = 0;
        listId = 0;

        if (!Sessions.TryGetValue(player.Id, out var session))
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

    public static void Clear(IPlayer player)
    {
        Sessions.Remove(player.Id);
    }

    private sealed class EditSession
    {
        public uint WindowTextId;
        public uint HouseId;
        public uint ListId;
    }
}
