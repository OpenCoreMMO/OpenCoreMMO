using System.Linq;
using NeoServer.Game.Common.Contracts.Items;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface IItemTypeStore : IDataStore<ushort, IItemType>
{
    public virtual IItemType GetByName(string name)
    {
        return All.FirstOrDefault(c => c.Name.ToLower().Equals(name.ToLower()));
    }
}