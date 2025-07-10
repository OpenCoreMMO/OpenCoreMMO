using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Spells;

public class HealSpell : Spell<HealSpell>
{
    public HealSpell(MinMax minMax, EffectT effect)
    {
        Effect = effect;
        Min = (ushort)minMax.Min;
        Max = (ushort)minMax.Max;
    }

    public override string Name => "Healing";
    public override EffectT Effect { get; } = EffectT.GlitterBlue;
    public override ushort ManaConsumption => 60;
    public override ConditionType ConditionType => ConditionType.None;
    public virtual ushort Min { get; }
    public virtual ushort Max { get; }
    public override uint Duration => 0;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        var hpToIncrease = GameRandom.Random.NextInRange(Min, Max);
        caster.Heal((ushort)hpToIncrease, caster);
        return Result.Success;
    }
}