using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Guild;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IGuildStore : IDataStore<ushort, Guild>
{
}