using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Containers.Container;

namespace NeoServer.Domain.Items.Factories;

public class ContainerFactory : IFactory
{
    public event CreateItem OnItemCreated;


    public IItem Create(IItemType itemType, Location location, IEnumerable<IItem> children)
    {
        if (Depot.Depot.IsApplicable(itemType)) return new Depot.Depot(itemType, location, children);
        if (Container.IsApplicable(itemType)) return new Container(itemType, location, children);

        return null;
    }
}