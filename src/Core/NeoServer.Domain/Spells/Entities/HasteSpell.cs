using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Spells.Entities;

public class HasteSpell : Spell<HasteSpell>
{
    public HasteSpell(uint duration, ushort speedBoost, EffectT effect)
    {
        Effect = effect;
        SpeedBoost = speedBoost;
        Duration = duration;
    }

    public HasteSpell()
    {
    }

    public override string Name => "Haste";
    public override EffectT Effect { get; } = EffectT.GlitterBlue;
    public override uint Duration { get; } = 10000;
    public virtual ushort SpeedBoost { get; } = 200;
    public override ushort ManaConsumption => 60;
    public override ConditionType ConditionType => ConditionType.Haste;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        caster.IncreaseSpeed(SpeedBoost);
        return Result.Success;
    }

    public override void OnEnd(ICombatActor actor)
    {
        actor.DecreaseSpeed(SpeedBoost);
        base.OnEnd(actor);
    }
}