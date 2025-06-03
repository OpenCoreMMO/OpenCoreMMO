using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Paladin;

public class DivineMissile : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Holy,
        Effect = EffectT.HolyDamage,
        ShootType = ShootType.SmallHoly
    };

    public override string Name => "Divine Missile";
    public override string Words => "exori san";
    public override ushort MinLevel => 40;
    public override ushort ManaConsumption { get; set; } = 20;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 2 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 4;
    public override string[] Vocations { get; } = ["paladin", "royal paladin"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + magicLevel * 1.79 + 11;
        var max = level / 5 + magicLevel * 3 + 18;

        return new MinMax(min, max);
    }
}