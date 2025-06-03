using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class IceWave : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Ice,
        Effect = EffectT.IceArea,
        BlockArmor = true
    };

    public override string Name => "Ice Wave";
    public override string Words => "exevo frigo hur";
    public override ushort MinLevel => 18;
    public override ushort ManaConsumption { get; set; } = 25;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 4 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["druid", "elder druid"];
    public override bool NeedDirection => true;
    protected override string AreaName => "AREA_WAVE4";

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + magicLevel * 0.81 + 4;
        var max = level / 5 + magicLevel * 2 + 12;

        return new MinMax(min, max);
    }
}