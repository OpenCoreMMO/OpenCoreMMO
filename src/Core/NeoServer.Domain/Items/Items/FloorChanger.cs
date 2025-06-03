using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class FloorChanger : BaseItem
{
    public FloorChanger(IItemType metadata, Location location) : base(metadata, location)
    {
    }

    public override void Use(IPlayer usedBy)
    {
        if (!usedBy.Location.IsNextTo(Location)) return;
        var toLocation = Location.Zero;

        var floorChange = Metadata.Attributes.GetAttribute(ItemAttribute.FloorChange);

        if (floorChange == "up") toLocation.Update(Location.X, Location.Y, (byte)(Location.Z - 1));
        if (floorChange == "down") toLocation.Update(Location.X, Location.Y, (byte)(Location.Z + 1));

        usedBy.TeleportTo(toLocation);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.HasAttribute(ItemAttribute.FloorChange) &&
               type.HasFlag(ItemFlag.Usable);
    }
}