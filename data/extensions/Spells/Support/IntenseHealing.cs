using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Support;

public class IntenseHealing : Spell<IntenseHealing>
{
    public override EffectT Effect => EffectT.GlitterBlue;

    public override uint Duration => 0;

    public override ConditionType ConditionType => ConditionType.None;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        caster.Heal(100, caster);
        return Result.Success;
    }
}