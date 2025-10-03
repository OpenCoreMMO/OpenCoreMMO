using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Player.Vocation;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IVocationStore : IDataStore<byte, Vocation>, IDataStore
{
    Vocation GetByName(string name);
}