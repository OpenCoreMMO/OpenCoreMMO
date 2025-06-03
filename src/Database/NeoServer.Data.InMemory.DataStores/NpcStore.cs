using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

public class NpcStore : DataStore<NpcStore, string, INpcType>, INpcStore
{
}