using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Common.Contracts.Combat;

public interface ICombatDefense : IProbability
{
    ushort Interval { get; init; }

    void Defend(ICombatActor actor);
}