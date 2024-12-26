using System;
using System.Collections.Generic;
using System.Globalization;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items.Cumulatives;

namespace NeoServer.Game.Items.Items.UsableItems.Runes;

public abstract class Rune : Cumulative, IRune
{
    protected Rune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
    }

    protected Rune(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }

    public abstract ushort Duration { get; }

    public Dictionary<string, (double, double)> Variables
    {
        get
        {
            var x = Metadata.Attributes.GetAttributeArray("x");
            var y = Metadata.Attributes.GetAttributeArray("y");

            x ??= new dynamic[] { "0", "0" };
            y ??= new dynamic[] { "0", "0" };

            var dictionary = new Dictionary<string, (double, double)>(2)
            {
                { "x", (x[0], x[1]) },
                { "y", (y[0], y[1]) }
            };
            return dictionary;
        }
    }

    public CooldownTime Cooldown { get; protected set; }

    public virtual MinMax Formula(IPlayer player, int level, int magicLevel)
    {
        var variables = Variables;
        variables.TryGetValue("x", out var x);
        variables.TryGetValue("y", out var y);

        var min = (int)(level / 5 + magicLevel * Math.Min(x.Item1, x.Item2) + Math.Min(y.Item1, y.Item2));
        var max = (int)(level / 5 + magicLevel * Math.Max(x.Item1, x.Item2) + Math.Min(y.Item1, y.Item2));

        return new MinMax(min, max);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Attributes.GetAttribute(ItemAttribute.Type)
            ?.Equals("rune", StringComparison.InvariantCultureIgnoreCase) ?? false;
    }
}