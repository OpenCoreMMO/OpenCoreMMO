using NeoServer.Domain.Creatures.Player;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IGroupStore : IDataStore<byte, Group>, IDataStore
{
}