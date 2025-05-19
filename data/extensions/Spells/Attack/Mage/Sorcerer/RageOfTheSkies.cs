using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Sorcerer;

public class RageOfTheSkies : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Energy,
        Effect = EffectT.BigClouds,
        Area = AreaEffect.Circle6X6,
    };
    public override string Name { get; set; } = "Rage of the Skies";
    public override string Words { get; set; } = "exevo gran mas vis";
    public override ushort MinLevel => 55;
    public override ushort Mana { get; set; } = 600;
    public override bool Premium => true;
    public override uint Cooldown => 40 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack, MagicGroup.Focus];
    public override uint[] GroupCooldown => [2 * 1000, 40 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["sorcerer", "master sorcerer"];
    protected override bool IsSelfTarget => true;
    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 7);
        var max = (level / 5) + (magicLevel * 14);

        return new MinMax(min, max);
    }
}