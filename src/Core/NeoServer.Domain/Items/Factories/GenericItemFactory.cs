using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Factories;

public class GenericItemFactory : IFactory
{
    public event CreateItem OnItemCreated;

    public IItem Create(IItemType itemType, Location location)
    {
        return new Item(itemType, location);
    }
}