using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface IGroupStore : IDataStore<byte, IGroup>, IDataStore
{
}