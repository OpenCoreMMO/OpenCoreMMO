using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Sorcerer;

public class EnergyWave : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Energy,
        Effect = EffectT.EnergyArea,
        ShootType = ShootType.Energy
    };

    public override string Name => "Energy Wave";
    public override string Words => "exevo vis hur";
    public override ushort MinLevel => 38;
    public override ushort Mana { get; set; } = 170;
    public override bool Premium => true;
    public override uint Cooldown => 8 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["sorcerer", "master sorcerer"];
    public override bool NeedDirection => true;
    protected override string AreaName => "AREA_SQUAREWAVE5";

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 4.5);
        var max = (level / 5) + (magicLevel * 9);

        return new MinMax(min, max);
    }
}