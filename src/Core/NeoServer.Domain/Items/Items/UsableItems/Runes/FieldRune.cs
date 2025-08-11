using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.UsableItems.Runes;

public class FieldRune : Rune, IUsableOnTile
{
    public FieldRune(IItemType type, Location location) : base(type, location)
    {
    }

    public FieldRune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    public ushort Field => Metadata.Attributes.GetAttribute<ushort>(ItemTypeAttribute.Field);

    public virtual string Area => Metadata.Attributes.GetAttribute(ItemTypeAttribute.Area);

    public bool Use(ICreature usedBy, ITile tile)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        OnUsedOnTile?.Invoke(usedBy, dynamicTile, this);

        Reduce();

        return true;
    }

    public new static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.FieldRune;
    }

    public static event UseOnTile OnUsedOnTile;
}