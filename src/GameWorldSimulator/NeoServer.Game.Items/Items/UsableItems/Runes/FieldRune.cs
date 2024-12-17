using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Items.Items.UsableItems.Runes;

public class FieldRune : Rune, IFieldRune
{
    public FieldRune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(
        type, location, attributes)
    {
    }

    public FieldRune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    public override ushort Duration => 2;
    public ushort Field => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Field);

    public virtual string Area => Metadata.Attributes.GetAttribute(ItemAttribute.Area);

    public bool Use(ICreature usedBy, ITile tile)
    {
        if (tile is not IDynamicTile dynamicTile) return false;
        OnUsedOnTile?.Invoke(usedBy, dynamicTile, this);

        Reduce();

        return true;
    }

    public static event UseOnTile OnUsedOnTile;

    public new static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.FieldRune;
    }
}