using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureBeforeDeathEvent(
    ICombatActor Creature,
    ICombatActor Killer,
    int RealDamage) : IEvent;
