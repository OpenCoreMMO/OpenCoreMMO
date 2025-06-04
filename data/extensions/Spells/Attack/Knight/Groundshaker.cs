using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Effects.Magical;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Attack.Knight;

public class Groundshaker : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.Skill, GetFormulaValues),
        DamageType = DamageType.Physical,
        Effect = EffectT.GroundShaker,
        Area = AreaEffect.Circle3X3,
        BlockArmor = true
    };

    public override string Name { get; set; } = "Groundshaker";
    public override string Words { get; set; } = "exori mas";
    public override ushort MinLevel => 33;
    public override ushort ManaConsumption { get; set; } = 160;
    public override bool NeedsPremium => true;
    public override bool NeedWeapon => true;
    public override uint Cooldown => 8 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["Knight", "Elite Knight"];
    protected override bool IsSelfTarget => true;

    private static MinMax GetFormulaValues(IPlayer player, int skill, int attack, decimal factor)
    {
        if (player is null) return MinMax.Zero;

        var level = player.Level;

        var min = level / 5 + (skill + attack) * 0.5;
        var max = level / 5 + (skill + attack) * 1.1;

        return new MinMax(min * 1.28f, max * 1.28);
    }
}