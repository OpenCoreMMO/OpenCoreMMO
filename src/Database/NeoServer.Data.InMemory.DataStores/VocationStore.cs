using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

public class VocationStore : DataStore<VocationStore, byte, IVocation>, IVocationStore
{
}