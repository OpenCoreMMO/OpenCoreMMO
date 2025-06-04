using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Items;

namespace NeoServer.Domain.Items.Factories;

public class GroundFactory : IFactory
{
    public event CreateItem OnItemCreated;


    public IItem Create(IItemType itemType, Location location)
    {
        if (Ground.IsApplicable(itemType)) return new Ground(itemType, location);

        return null;
    }
}