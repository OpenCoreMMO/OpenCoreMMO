using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Guild;

namespace NeoServer.Data.InMemory.DataStores;

public class GuildStore : DataStore<GuildStore, ushort, Guild>, IGuildStore
{
}