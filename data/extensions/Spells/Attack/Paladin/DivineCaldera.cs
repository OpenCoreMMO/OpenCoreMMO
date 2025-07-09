using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Attack.Paladin;

public class DivineCaldera : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (FormulaType.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Holy,
        Effect = EffectT.HolyArea,
        Area = AreaEffect.Circle3X3
    };

    public override string Name { get; set; } = "Divine Caldera";
    public override string Words { get; set; } = "exevo mas san";
    public override ushort MinLevel => 50;
    public override ushort ManaConsumption { get; set; } = 160;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 4 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["paladin", "royal paladin"];
    protected override bool IsSelfTarget => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + magicLevel * 4;
        var max = level / 5 + magicLevel * 6;

        return new MinMax(min, max);
    }
}