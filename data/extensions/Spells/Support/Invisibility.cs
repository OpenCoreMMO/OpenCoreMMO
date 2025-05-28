using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

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