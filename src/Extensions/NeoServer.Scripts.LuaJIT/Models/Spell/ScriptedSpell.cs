using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Spells.Entities;

namespace NeoServer.Scripts.LuaJIT.Models.Spell;

public abstract class ScriptedSpell : Spell<ScriptedSpell>
{
    public override uint Duration { get; }
    public override ConditionType ConditionType { get; }
    public override EffectT Effect { get; }
}