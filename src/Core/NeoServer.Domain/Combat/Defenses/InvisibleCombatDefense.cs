using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells;
using NeoServer.Domain.Spells.Entities;

namespace NeoServer.Domain.Combat.Defenses;

public class InvisibleCombatDefense : BaseCombatDefense
{
    public InvisibleCombatDefense(uint duration, EffectT effect)
    {
        Spell = new InvisibleSpell(duration, effect);
    }

    public ISpell Spell { get; }

    public override void Defend(ICombatActor actor)
    {
        Spell?.Invoke(actor, null, false);
    }
}