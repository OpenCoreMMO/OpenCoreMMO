using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items.Cumulatives;

namespace NeoServer.Game.Items.Items.UsableItems;

public class Food : Cumulative, IConsumable, IFood
{
    public Food(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    public Food(IItemType type, Location location, byte amount) : base(type,
        location, amount)
    {
    }

    public int CooldownTime => 0;

    public void Use(IPlayer usedBy, ICreature creature)
    {
        if (creature is not IPlayer player) return;

        if (!player.Feed(this)) return;

        Reduce();
        IConsumable.RaiseOnUsed(usedBy, creature, this);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Food;
    }
}