using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells;

namespace NeoServer.Domain.Combat.Defenses;

public class IllusionCombatDefense : BaseCombatDefense
{
    public IllusionCombatDefense(uint duration, string monsterName, EffectT effect,
        IMonsterTypeStore monsterTypeStore) //todo: remove dataManager from here
    {
        Spell = new IllusionSpell(duration, monsterName, monsterTypeStore, effect);
    }

    public ISpell Spell { get; }

    public override void Defend(ICombatActor actor)
    {
        Spell?.Invoke(actor, null, false);
    }
}