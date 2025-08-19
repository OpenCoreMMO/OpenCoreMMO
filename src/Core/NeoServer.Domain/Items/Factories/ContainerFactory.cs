using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Depot;
using NeoServer.Domain.Items.Items.Containers.Container;

namespace NeoServer.Domain.Items.Factories;

public class ContainerFactory() : IFactory
{
    public event CreateItem OnItemCreated;

    public IItem Create(IItemType itemType, Location location, IEnumerable<IItem> children)
    {
        if (Locker.IsApplicable(itemType))
        {
            return new Locker(itemType, location, children);
        }

        if (Container.IsApplicable(itemType)) return new Container(itemType, location, children);

        return null;
    }
}