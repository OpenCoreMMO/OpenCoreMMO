using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface INpcTypeStore : IDataStore<string, INpcType>
{
    public virtual INpcType GetByName(string name)
    {
        return All.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }
}