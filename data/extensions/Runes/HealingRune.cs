using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items.UsableItems.Runes;

namespace NeoServer.Extensions.Runes;

public class HealingRune : Rune, IConsumable
{
    public HealingRune(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) :
        base(type, location, attributes)
    {
    }

    public override ushort Duration => 0;
    public EffectT Effect => EffectT.GlitterBlue;

    public void Use(IPlayer usedBy, ICreature creature)
    {
        var minMax = Formula(usedBy, usedBy.Level, usedBy.GetSkillLevel(SkillType.Magic));
        var healValue = (ushort)GameRandom.Random.Next(minMax.Min, maxValue: minMax.Max);
        if (creature is ICombatActor actor) actor.Heal(healValue, usedBy);

        Reduce();

        IConsumable.RaiseOnUsed(usedBy, creature, this);
    }
}