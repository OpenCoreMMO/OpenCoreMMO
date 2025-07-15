using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.UsableItems;

public class FloorChangerUsableItem : UsableOnItem, IUsableOnItem
{
    public FloorChangerUsableItem(IItemType type, Location location) : base(type, location)
    {
    }

    public override bool AllowUseOnDistance => false;

    public virtual bool Use(ICreature usedBy, IItem onItem)
    {
        if (usedBy is not IPlayer player) return false;
        var canUseOnItems = Metadata.OnUse?.GetAttributeArray<ushort>(ItemTypeAttribute.UseOn) ?? Array.Empty<ushort>();

        if (!canUseOnItems.Contains(onItem.Metadata.ServerId)) return false;

        if (Metadata.OnUse?.GetAttribute(ItemTypeAttribute.FloorChange) != "up") return false;

        var toLocation = new Location(onItem.Location.X, onItem.Location.Y, (byte)(onItem.Location.Z - 1));

        player.TeleportTo(toLocation);
        return true;
    }

    public new static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.UsableFloorChanger;
    }
}