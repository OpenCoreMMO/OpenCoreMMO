using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Spells;

namespace NeoServer.Extensions.Spells.Support;

public class Food : Spell<Food>
{
    public override EffectT Effect => EffectT.GlitterGreen;

    public override uint Duration => 0;

    public override ConditionType ConditionType => ConditionType.None;

    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        return Result.Success;
    }
}