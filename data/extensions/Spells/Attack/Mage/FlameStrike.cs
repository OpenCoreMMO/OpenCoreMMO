using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage;

public class FlameStrike : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Fire,
        Effect = EffectT.FireAttack,
        ShootType = ShootType.Fire
    };

    public override string Name => "Flame Strike";
    public override string Words => "exori flam";
    public override ushort MinLevel => 12;
    public override ushort Mana { get; set; } = 20;
    public override bool Premium => true;
    public override uint Cooldown => 2 * 1000;
    public override SpellGroup[] Groups { get; } = [SpellGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte Range => 3;
    public override string[] Vocations { get; } = ["druid", "elder druid", "sorcerer", "master sorcerer"];
    public override bool CasterNeedsTargetOrDirection => true;
    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 1.403) + 8;
        var max = (level / 5) + (magicLevel * 2.203) + 13;

        return new MinMax(min, max);
    }
}