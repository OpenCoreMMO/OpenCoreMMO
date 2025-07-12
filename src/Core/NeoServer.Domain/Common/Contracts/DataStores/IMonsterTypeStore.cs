using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IMonsterTypeStore : IDataStore<string, IMonsterType>
{
}