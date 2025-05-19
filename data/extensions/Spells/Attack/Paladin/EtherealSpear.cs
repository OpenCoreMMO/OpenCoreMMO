using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Paladin;

public class EtherealSpear : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.Skill, GetFormulaValues),
        DamageType = DamageType.Physical,
        Effect = EffectT.XGray,
        ShootType = ShootType.EtherealSpear,
        BlockArmor = true
    };

    public override string Name => "Ethereal Spear";
    public override string Words => "exori con";
    public override ushort MinLevel => 23;
    public override ushort Mana { get; set; } = 25;
    public override bool Premium => true;
    public override uint Cooldown => 2 * 1000;
    public override bool NeedsTarget => true;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte Range => 7;
    public override string[] Vocations { get; } = ["paladin", "royal paladin"];

    private static MinMax GetFormulaValues(IPlayer player, int skill, int attack, decimal factor)
    {
        if (player is null) return MinMax.Zero;

        var level = player.Level;

        var min = (level / 5) + (skill * 25) / 3;
        var max = (level / 5) + skill + 25;

        return new MinMax(min, max);
    }
}