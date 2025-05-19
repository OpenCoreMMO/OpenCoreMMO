using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Knight;

public class WhirlwindThrow : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageType = DamageType.Physical,
        ShootType = ShootType.WeaponType,
        Effect = EffectT.XGray,
        BlockArmor = true,
        DamageFormula = (CombatFormula.Skill, GetFormulaValues)
    };
    public override string Name { get; set; } = "Whirlwind Throw";
    public override string Words { get; set; } = "exori hur";
    public override ushort MinLevel => 28;
    public override ushort Mana { get; set; } = 40;
    public override bool Premium => true;
    public override byte Range => 5;
    public override bool BlockWalls => true;
    public override bool NeedWeapon => true;
    public override bool NeedsTarget => true;
    public override uint Cooldown => 6 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["Knight", "Elite Knight"];
    private static MinMax GetFormulaValues(IPlayer player, int skill, int attack, decimal factor)
    {
        if (player is null) return MinMax.Zero;

        var level = player.Level;

        var min = (level / 5) + (skill + attack) / 3;
        var max = (level / 5) + (skill + attack);

        return new MinMax(min * 1.28f, max * 1.28);
    }
}