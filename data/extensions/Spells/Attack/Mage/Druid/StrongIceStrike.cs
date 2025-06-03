using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class StrongIceStrike : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Ice,
        Effect = EffectT.IceAttack,
        ShootType = ShootType.SmallIce
    };

    public override string Name => "Strong Ice Wave";
    public override string Words => "exori gran frigo";
    public override ushort MinLevel => 80;
    public override ushort ManaConsumption { get; set; } = 60;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 8 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack, MagicGroup.Special];
    public override uint[] GroupCooldown => [2 * 1000, 8 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 3;
    public override string[] Vocations { get; } = ["druid", "elder druid"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + +(magicLevel * 2.8) + 16;
        var max = level / 5 + +(magicLevel * 4.4) + 28;

        return new MinMax(min, max);
    }
}