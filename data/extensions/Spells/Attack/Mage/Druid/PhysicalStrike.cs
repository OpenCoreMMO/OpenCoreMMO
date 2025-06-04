using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class PhysicalStrike : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Physical,
        Effect = EffectT.DamageExplosion,
        ShootType = ShootType.Explosion
    };

    public override string Name => "Physical Strike";
    public override string Words => "exori moe ico";
    public override ushort MinLevel => 16;
    public override ushort ManaConsumption { get; set; } = 20;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 2 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 3;
    public override string[] Vocations { get; } = ["druid", "elder druid"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + +(magicLevel * 1.6) + 9;
        var max = level / 5 + +(magicLevel * 2.4) + 14;

        return new MinMax(min, max);
    }
}