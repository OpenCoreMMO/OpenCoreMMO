using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Sorcerer;

public class DeathStrike : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Death,
        Effect = EffectT.BubbleBlack,
        ShootType = ShootType.Death
    };

    public override string Name => "Death Strike";
    public override string Words => "exori mort";
    public override ushort MinLevel => 16;
    public override ushort ManaConsumption { get; set; } = 20;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 2 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 3;
    public override string[] Vocations { get; } = ["sorcerer", "master sorcerer"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + magicLevel * 1.403 + 8;
        var max = level / 5 + magicLevel * 2.203 + 13;

        return new MinMax(min, max);
    }
}