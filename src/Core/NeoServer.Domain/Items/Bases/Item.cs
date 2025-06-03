using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Bases;

public class Item : BaseItem
{
    public Item(IItemType metadata, Location location) : base(metadata, location)
    {
    }
}