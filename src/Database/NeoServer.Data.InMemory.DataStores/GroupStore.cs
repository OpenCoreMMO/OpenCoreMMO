using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Group;

namespace NeoServer.Data.InMemory.DataStores;

public class GroupStore : DataStore<GroupStore, byte, Group>, IGroupStore
{
}