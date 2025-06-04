using NeoServer.Domain.Creatures.Group;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IGroupStore : IDataStore<byte, Group>, IDataStore
{
}