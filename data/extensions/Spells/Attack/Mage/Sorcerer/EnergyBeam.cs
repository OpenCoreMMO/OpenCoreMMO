using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Extensions.Spells.Attack.Mage.Sorcerer;

public partial class EnergyBeam : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (CombatFormula.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Energy,
        Effect = EffectT.DamageEnergy,
    };

    public override string Name => "Energy Beam";
    public override string Words => "exevo vis lux";
    public override ushort MinLevel => 23;
    public override ushort Mana { get; set; } = 40;
    public override bool Premium => true;
    public override uint Cooldown => 4 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override string[] Vocations { get; } = ["sorcerer", "master sorcerer"];
    public override bool NeedDirection => true;
    protected override string AreaName => "AREA_BEAM5";

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        var min = (level / 5) + (magicLevel * 1.8) + 11;
        var max = (level / 5) + (magicLevel * 3) + 19;

        return new MinMax(min, max);
    }
}