using System;
using System.Collections.Generic;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Spell;
using NeoServer.Game.Items.Items.UsableItems.Runes;

namespace NeoServer.Extensions.Runes;

public class SuddenDeath(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes)
    : AttackRune(type, location, attributes)
{
    public CombatParameter CombatParameter = new()
    {
        DamageType = DamageType.Death,
        Effect = EffectT.BubbleBlack,
        ShootType = ShootType.Death,
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues)
    };
    
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public ushort MinLevel => 45;
    public ushort MinMagicLevel => 15;
    public uint Cooldown => 2 * 1000;
    public uint[] GroupCooldown => [2 * 1000];
    public bool IsBlocking => true;

    public override bool NeedTarget => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 4.605) + 28;
        var max = (level / 5) + (magicLevel * 7.395) + 46;

        return new MinMax(min, max);
    }
}