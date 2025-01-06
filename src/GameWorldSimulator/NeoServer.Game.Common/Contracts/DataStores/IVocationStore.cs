using NeoServer.Game.Common.Contracts.Creatures;
using System.Linq;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface IVocationStore : IDataStore<byte, IVocation>, IDataStore
{
    public virtual IVocation GetByName(string name)
    {
        return All.FirstOrDefault(c => c.Name.ToLower().Equals(name.ToLower()));
    }
}