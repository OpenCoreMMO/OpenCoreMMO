using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Guild;

namespace NeoServer.Data.InMemory.DataStores;

public class GuildStore : DataStore<GuildStore, ushort, Guild>, IGuildStore
{
}