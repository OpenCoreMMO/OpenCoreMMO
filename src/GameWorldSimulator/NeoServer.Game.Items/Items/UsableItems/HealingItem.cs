using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items.Cumulatives;

namespace NeoServer.Game.Items.Items.UsableItems;

public class HealingItem : Cumulative, IConsumable
{
    public HealingItem(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) :
        base(type, location, attributes)
    {
    }

    public ushort Min => Metadata.Attributes.GetInnerAttributes(ItemAttribute.Healing)
        ?.GetAttribute<ushort>(ItemAttribute.Min) ?? 0;

    public ushort Max => Metadata.Attributes.GetInnerAttributes(ItemAttribute.Healing)
        ?.GetAttribute<ushort>(ItemAttribute.Max) ?? 0;

    public string Type => Metadata.Attributes.GetAttribute(ItemAttribute.Healing);

    public void Use(IPlayer usedBy, ICreature creature)
    {
        if (creature is not ICombatActor actor) return;
        if (Max == 0) return;

        var value = (ushort)GameRandom.Random.Next(Min, maxValue: Max);

        if (Type.Equals("hp", StringComparison.InvariantCultureIgnoreCase))
            actor.Heal(value, usedBy);
        else if (creature is IPlayer player) player.HealMana(value);

        Reduce();

        IConsumable.RaiseOnUsed(usedBy, creature, this);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Healing;
    }
}