using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class TeleportItem : BaseItem
{
    public TeleportItem(IItemType metadata, Location location) : base(metadata, location)
    {
    }

    private Location Destination =>
        Attributes.TryGetValue(ItemAttribute.TeleportDestination, out var destination) &&
        destination is Location destLocation
            ? destLocation
            : Location.Zero;

    public bool HasDestination => Destination != Location.Zero;

    public bool Teleport(IWalkableCreature player)
    {
        if (!HasDestination) return false;
        player.TeleportTo(Destination);
        return true;
    }

    public static bool IsApplicable(IItemType type)
    {
        return type
            .Attributes
            .GetAttribute(ItemTypeAttribute.Type)
            ?.Equals("teleport", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}