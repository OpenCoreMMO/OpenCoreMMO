using NeoServer.Domain.Combat.Spells;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;

namespace NeoServer.Domain.Combat.Defenses;

public class HealCombatDefense : BaseCombatDefense
{
    public HealCombatDefense(int min, int max, EffectT effect) //todo: remove dataManager from here
    {
        Spell = new HealSpell(new MinMax(min, max), effect);
    }

    public ISpell Spell { get; }

    public override void Defend(ICombatActor actor)
    {
        Spell?.Invoke(actor, null, false);
    }
}