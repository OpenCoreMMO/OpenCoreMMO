using System;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Attack.Mage.Druid;

public class MudAttack : AttackSpell
{
    protected override CombatParameter CombatSettings { get; } = new()
    {
        DamageFormula = (FormulaType.MagicLevel, GetFormulaValues),
        DamageType = DamageType.Earth,
        Effect = EffectT.Carniphila,
        ShootType = ShootType.SmallEarth
    };

    public override string Name => "Mud Attack";
    public override string Words => "exori infir tera";
    public override ushort MinLevel => 1;
    public override ushort ManaConsumption { get; set; } = 6;
    public override bool NeedsPremium => false;
    public override uint Cooldown => 2 * 1000;
    public override MagicGroup[] Groups { get; } = [MagicGroup.Attack];
    public override uint[] GroupCooldown => [2 * 1000];
    public override bool NeedLearn => false;
    public override byte? Range => 3;
    public override string[] Vocations { get; } = ["druid", "elder druid"];
    public override bool CasterNeedsTargetOrDirection => true;

    private static MinMax GetFormulaValues(IPlayer player, int level, int magicLevel, decimal _)
    {
        if (player is null) return MinMax.Zero;

        level = Math.Min(level, 20);
        magicLevel = Math.Min(magicLevel, 20);

        var min = level / 5 + +(magicLevel * 0.4) + 2;
        var max = level / 5 + +(magicLevel * 0.8) + 5;

        return new MinMax(min, max);
    }
}