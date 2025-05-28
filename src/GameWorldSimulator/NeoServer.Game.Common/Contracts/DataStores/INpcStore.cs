using NeoServer.Game.Common.Contracts.Creatures;
using System;
using System.Linq;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface INpcStore : IDataStore<string, INpcType>
{
    public virtual INpcType GetByName(string name)
    {
        return All.FirstOrDefault(c => c.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }
}