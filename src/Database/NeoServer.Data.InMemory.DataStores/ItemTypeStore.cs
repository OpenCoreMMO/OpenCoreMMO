using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Data.InMemory.DataStores;

public class ItemTypeStore : DataStore<ItemTypeStore, ushort, IItemType>, IItemTypeStore
{
}