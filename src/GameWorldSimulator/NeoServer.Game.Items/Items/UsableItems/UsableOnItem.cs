using System.Collections;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items.UsableItems;

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