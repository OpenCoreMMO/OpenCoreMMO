using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Runes;

public interface IRune : IHasCooldown, ICumulative, IUsableRequirement
{
    bool Enabled { get; }
    ISpell Spell { get; }
    Result CanBeCastBy(ICombatActor caster, IThing target);
    void PostUse(bool reduce = true);
}