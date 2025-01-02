using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;

namespace NeoServer.Data.InMemory.DataStores;

public class GroupStore : DataStore<GroupStore, byte, IGroup>, IGroupStore
{
}