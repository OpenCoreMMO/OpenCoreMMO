using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Results;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public abstract class ScriptedSpell: Game.Combat.Spells.Spell<ScriptedSpell>
{
    public override uint Duration { get; }
    public override ConditionType ConditionType { get; }
    public override EffectT Effect { get; }
}