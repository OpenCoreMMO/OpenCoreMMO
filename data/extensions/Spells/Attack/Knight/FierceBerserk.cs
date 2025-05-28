using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Knight;

public class FierceBerserk : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.Skill, GetFormulaValues),
        DamageType = DamageType.Physical,
        Effect = EffectT.XGray,
        Area = AreaEffect.Square1X1,
        BlockArmor = true,
    };
    public override string Name { get; set; } = "Fierce Berserk";
    public override string Words { get; set; } = "exori gran";
    public override ushort MinLevel => 90;
    public override ushort ManaConsumption { get; set; } = 340;
    public override bool NeedsPremium => true;
    public override bool NeedWeapon => true;
    public override uint Cooldown => 6 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack]; 
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["Knight", "Elite Knight"];
    protected override bool IsSelfTarget => true;
    private static MinMax GetFormulaValues(IPlayer player, int skill, int attack, decimal factor)
    {
        if (player is null) return MinMax.Zero;

        var level = player.Level;

        var min = (level / 5) + (skill + 2 * attack) * 1.1;
        var max = (level / 5) + (skill + 2 * attack) * 3;

        return new MinMax(min * 1.1f, max * 1.1);
    }
}