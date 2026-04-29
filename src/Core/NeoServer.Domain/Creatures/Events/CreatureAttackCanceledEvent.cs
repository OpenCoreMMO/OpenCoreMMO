using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Events;

public record CreatureAttackCanceledEvent(
    ICombatActor Actor) : IEvent;
