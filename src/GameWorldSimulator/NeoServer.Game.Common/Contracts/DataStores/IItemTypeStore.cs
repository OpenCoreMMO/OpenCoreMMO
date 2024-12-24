using NeoServer.Game.Common.Contracts.Items;
using System.Linq;

namespace NeoServer.Game.Common.Contracts.DataStores;

public interface IItemTypeStore : IDataStore<ushort, IItemType>
{
    public virtual IItemType GetByName(string name)
        => All.FirstOrDefault(c => c.Name.ToLower().Equals(name.ToLower()));
}