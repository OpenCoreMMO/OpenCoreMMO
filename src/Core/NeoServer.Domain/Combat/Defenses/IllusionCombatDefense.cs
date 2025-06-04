using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Spells;

namespace NeoServer.Domain.Combat.Defenses;

public class IllusionCombatDefense : BaseCombatDefense
{
    public IllusionCombatDefense(uint duration, string monsterName, EffectT effect,
        IMonsterDataManager dataManager) //todo: remove dataManager from here
    {
        Spell = new IllusionSpell(duration, monsterName, dataManager, effect);
    }

    public ISpell Spell { get; }

    public override void Defend(ICombatActor actor)
    {
        Spell?.Invoke(actor, null, false);
    }
}