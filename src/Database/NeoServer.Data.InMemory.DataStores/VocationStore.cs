using System;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Creatures.Player.Vocation;

namespace NeoServer.Data.InMemory.DataStores;

public class VocationStore : DataStore<VocationStore, byte, Vocation>, IVocationStore
{
    public Vocation GetByName(string name)
    {
        foreach (var vocation in All)
        {
            if (vocation.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return vocation;
            }
        }

        return null;
    }
}