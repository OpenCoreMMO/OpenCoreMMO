using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Defenses;

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