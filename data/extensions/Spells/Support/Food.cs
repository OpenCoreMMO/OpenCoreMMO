using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Extensions.Spells.Support;

public class Food : Spell<Food>
{
    public override EffectT Effect => EffectT.GlitterGreen;
    public override Result OnCast(ICombatActor caster, IThing target, bool isHotkey)
    {
        return Result.Success;
    }

    public override uint Duration => 0;

    public override ConditionType ConditionType => ConditionType.None;
}