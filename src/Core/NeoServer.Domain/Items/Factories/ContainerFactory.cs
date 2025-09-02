using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Items.Items.Containers.Container;

namespace NeoServer.Domain.Items.Factories;

public class ContainerFactory() : IFactory
{
    public event CreateItem OnItemCreated;

    public IItem Create(IItemType itemType, Location location, IEnumerable<IItem> children)
    {
        if (Locker.Locker.IsApplicable(itemType))
        {
            return new Locker.Locker(itemType, location, children);
        }

        if (itemType.Attributes.GetAttribute(ItemTypeAttribute.Type) == "mailbox" && location.Type != LocationType.Ground)
        {
            itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, (byte)30);
            return new Container(itemType, location, children)
            {
                CanMoveItemsToItself = false
            };
        }

        if (itemType.ServerId == GameConstants.PARCEL_SERVER_ID)
        {
            return new Parcel(itemType, location, children);
        }

        if (Container.IsApplicable(itemType))
        {
            return new Container(itemType, location, children);
        }

        return null;
    }
}