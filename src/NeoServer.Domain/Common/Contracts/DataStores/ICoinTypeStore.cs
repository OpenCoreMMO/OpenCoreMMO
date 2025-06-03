using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface ICoinTypeStore : IDataStore<ushort, IItemType>
{
}