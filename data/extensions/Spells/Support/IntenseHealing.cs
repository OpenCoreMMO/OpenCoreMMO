using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Extensions.Spells.Support;

public class IntenseHealing : Spell<IntenseHealing>
{
    public override EffectT Effect => EffectT.GlitterBlue;
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        caster.Heal(100, caster);
        return Result.Success;
    }

    public override uint Duration => 0;

    public override ConditionType ConditionType => ConditionType.None;
}