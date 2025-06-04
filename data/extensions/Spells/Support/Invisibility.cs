using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Support;

public class Invisibility : Spell<Invisibility>
{
    public override EffectT Effect => EffectT.GlitterBlue;
    public override uint Duration => 20000;
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