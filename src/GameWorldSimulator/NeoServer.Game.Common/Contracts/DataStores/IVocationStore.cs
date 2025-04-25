using System;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.DataStores;

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