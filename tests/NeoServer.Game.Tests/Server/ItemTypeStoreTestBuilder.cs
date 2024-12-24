using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Tests.Server;

public static class ItemTypeStoreTestBuilder
{
    public static ItemTypeStore Build(params IItem[] items)
    {
        var itemTypeStore = new ItemTypeStore();
        foreach (var item in items) itemTypeStore.Add(item.ServerId, item.Metadata);

        return itemTypeStore;
    }

    public static ItemTypeStore Build(params IItemType[] itemsTypes)
    {
        var itemTypeStore = new ItemTypeStore();
        foreach (var item in itemsTypes) itemTypeStore.Add(item.ServerId, item);

        return itemTypeStore;
    }
}