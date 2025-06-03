using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Data.InMemory.DataStores;

public class ItemTextWindowStore
{
    private static readonly Dictionary<uint, List<IItem>> playerItemsTextWindow = new();

    public static uint Add(IPlayer player, IItem item)
    {
        if (playerItemsTextWindow.TryGetValue(player.Id, out var items))
        {
            for (var i = 0; i < items.Count; i++)
                if (items[i] == item)
                    return (uint)i;

            items.Add(item);
            return (uint)items.Count - 1;
        }

        playerItemsTextWindow.Add(player.Id, new List<IItem>
        {
            item
        });

        return 0;
    }

    public static bool Get(IPlayer player, uint textWindowId, out IItem item)
    {
        item = null;
        if (!playerItemsTextWindow.TryGetValue(player.Id, out var items)) return false;

        if (textWindowId >= items.Count) return false;

        item = items[(int)textWindowId];
        return true;
    }
}