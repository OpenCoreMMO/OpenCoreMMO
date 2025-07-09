using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Spells;

public class InvisibleSpell : Spell<InvisibleSpell>
{
    public InvisibleSpell(uint duration)
    {
        Duration = duration;
    }

    public InvisibleSpell(uint duration, EffectT effect)
    {
        Duration = duration;
        Effect = effect;
    }

    public override string Name => "Invisible";
    public override EffectT Effect { get; } = EffectT.GlitterBlue;
    public override uint Duration { get; } = 10000;
    public override ushort ManaConsumption => 60;
    public override ConditionType ConditionType => ConditionType.Invisible;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        caster.TurnInvisible();
        return Result.Success;
    }

    public override void OnEnd(ICombatActor actor)
    {
        actor.TurnVisible();
        base.OnEnd(actor);
    }
}