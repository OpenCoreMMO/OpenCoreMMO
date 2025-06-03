using NeoServer.Domain.Combat.Spells;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;

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