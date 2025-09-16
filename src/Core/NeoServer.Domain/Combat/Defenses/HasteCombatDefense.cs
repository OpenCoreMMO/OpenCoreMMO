using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells.Entities;

namespace NeoServer.Domain.Combat.Defenses;

public class HasteCombatDefense : BaseCombatDefense
{
    public HasteCombatDefense(uint duration, ushort speedBoost, EffectT effect)
    {
        Spell = new HasteSpell(duration, speedBoost, effect);
    }

    public ISpell Spell { get; }

    public override void Defend(ICombatActor actor)
    {
        Spell?.Invoke(actor, null, false);
    }
}