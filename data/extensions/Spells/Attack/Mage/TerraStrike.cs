using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Attack.Mage;

public class TerraStrike : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (FormulaType.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Earth,
        Effect = EffectT.Carniphila,
        ShootType = ShootType.SmallEarth
    };

    public override string Name => "Terra Strike";
    public override string Words => "exori tera";
    public override ushort MinLevel => 12;
    public override ushort ManaConsumption { get; set; } = 20;
    public override bool NeedsPremium => true;
    public override uint Cooldown => 2 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 3;
    public override string[] Vocations { get; } = ["druid", "elder druid", "sorcerer", "master sorcerer"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = level / 5 + magicLevel * 1.403 + 8;
        var max = level / 5 + magicLevel * 2.203 + 13;

        return new MinMax(min, max);
    }
}