using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Defenses;

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