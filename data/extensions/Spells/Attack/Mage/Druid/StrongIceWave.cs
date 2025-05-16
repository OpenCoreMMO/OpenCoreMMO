using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class StrongIceWave : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        Type = AttackType.Spell,
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Ice,
        Effect = EffectT.IceArea,
        BlockArmor = true
    };

    public override string Name => "Strong Ice Wave";
    public override string Words => "exevo gran frigo hur";
    public override ushort MinLevel => 40;
    public override ushort Mana { get; set; } = 170;
    public override bool Premium => true;
    public override uint Cooldown => 8 * 1000;
    public override SpellGroup[] Groups { get; } = [SpellGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["druid", "elder druid"];
    public override bool NeedDirection => true;
    protected override string AreaName => "AREA_SHORTWAVE3";

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 4.5) + 20;
        var max = (level / 5) + (magicLevel * 7.6) + 48;

        return new MinMax(min, max);
    }
}