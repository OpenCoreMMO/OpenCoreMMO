using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public abstract class ScriptedSpell : Spell<ScriptedSpell>
{
    public override uint Duration { get; }
    public override ConditionType ConditionType { get; }
    public override EffectT Effect { get; }
}