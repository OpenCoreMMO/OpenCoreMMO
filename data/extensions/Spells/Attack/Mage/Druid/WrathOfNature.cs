using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class WrathOfNature : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Earth,
        Effect = EffectT.SmallPlants,
        Area = AreaEffect.Circle6X6
    };

    public override string Name => "Wrath of Nature";
    public override string Words => "exevo gran mas tera";
    public override ushort MinLevel => 55;
    public override ushort Mana { get; set; } = 700;
    public override bool Premium => true;
    public override uint Cooldown => 40 * 1000;
    protected override bool IsSelfTarget => true;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [4 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["druid", "elder druid"];

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 5);
        var max = (level / 5) + (magicLevel * 10);

        return new MinMax(min, max);
    }
}