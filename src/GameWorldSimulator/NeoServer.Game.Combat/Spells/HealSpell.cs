using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Spells;

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