using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;

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
    public override ushort Mana => 60;
    public override ConditionType ConditionType => ConditionType.None;
    public virtual ushort Min { get; }
    public virtual ushort Max { get; }
    public override uint Duration => 0;

    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        error = InvalidOperation.None;

        var hpToIncrease = GameRandom.Random.NextInRange(Min, Max);
        caster.Heal((ushort)hpToIncrease, caster);
        return true;
    }

    public override void OnEnd(ICombatActor actor)
    {
        base.OnEnd(actor);
    }
}