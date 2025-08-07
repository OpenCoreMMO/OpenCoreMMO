using NeoServer.Domain.Common.Contracts.Items;

namespace NeoServer.Domain.Common.Contracts.DataStores;

public interface IItemTypeStore : IDataStore<ushort, IItemType>
{
    public virtual IItemType GetByName(string name)
    {
        foreach (var itemType in All)
            if (!string.IsNullOrEmpty(itemType.Name) &&
                itemType.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return itemType;

        return null;
    }
}