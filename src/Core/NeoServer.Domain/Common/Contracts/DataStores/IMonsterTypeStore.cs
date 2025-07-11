using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IMonsterTypeStore : IDataStore<string, IMonsterType>
{
    public virtual IMonsterType GetByName(string name)
    {
        return All.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }
}