using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Common.Contracts.Items.Types.Runes;

public interface IRune : IHasCooldown, ICumulative, IUsableRequirement
{
    bool Enabled { get; }
    ISpell Spell { get; }
    Result CanBeCastBy(ICombatActor caster, IThing target);
    void PostUse(bool reduce = true);
}