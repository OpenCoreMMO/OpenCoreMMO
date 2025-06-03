using NeoServer.Game.Combat.Spells;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Defenses;

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