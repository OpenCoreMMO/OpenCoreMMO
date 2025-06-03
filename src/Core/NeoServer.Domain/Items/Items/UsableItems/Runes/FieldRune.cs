using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Runes;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Items.Items.UsableItems.Runes;

public class FieldRune : Rune, IFieldRune
{
    public FieldRune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(
        type, location, attributes)
    {
    }

    public FieldRune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    public ushort Field => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Field);

    public virtual string Area => Metadata.Attributes.GetAttribute(ItemAttribute.Area);

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