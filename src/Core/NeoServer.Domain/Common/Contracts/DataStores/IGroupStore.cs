using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IGroupStore : IDataStore<byte, IGroup>, IDataStore
{
}