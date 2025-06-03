using System.Collections;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.UsableItems;

public class UsableOnItem : BaseItem, IUsableOnItem
{
    public UsableOnItem(IItemType type, Location location) : base(type, location)
    {
    }

    public virtual bool AllowUseOnDistance => false;

    public virtual bool CanUseOn(IItem onItem)
    {
        var useOnItems = Metadata.OnUse?.GetAttributeArray<ushort>(ItemAttribute.UseOn);

        return useOnItems is not null && useOnItems.Contains(onItem.Metadata.ServerId);
    }

    public bool CanUseOn(ushort[] items, IItem onItem)
    {
        return ((IList)items)?.Contains(onItem.Metadata.ServerId) ?? false;
    }

    public virtual bool CanUse(ICreature usedBy, IItem onItem)
    {
        if (!AllowUseOnDistance && !usedBy.Location.IsNextTo(onItem.Location)) return false;
        return usedBy.Location.SameFloorAs(onItem.Location);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.UsableOn;
    }
}