using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IVocationStore : IDataStore<byte, IVocation>, IDataStore
{
    public virtual IVocation GetByName(string name)
    {
        foreach (var vocation in All)
            if (vocation.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return vocation;
        return null;
    }
}