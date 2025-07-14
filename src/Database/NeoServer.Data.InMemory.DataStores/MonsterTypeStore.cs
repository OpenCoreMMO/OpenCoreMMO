using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

public class MonsterTypeStore : DataStore<MonsterTypeStore, string, IMonsterType>, IMonsterTypeStore
{
}