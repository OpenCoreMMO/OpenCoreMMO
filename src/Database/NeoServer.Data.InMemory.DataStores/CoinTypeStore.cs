using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Data.InMemory.DataStores;

public class CoinTypeStore : DataStore<CoinTypeStore, ushort, IItemType>, ICoinTypeStore
{
}