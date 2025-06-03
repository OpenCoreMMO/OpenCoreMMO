using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Models.Bases.Events;

public record CreatureHealthChangedEvent(ICombatActor Actor, uint OldHealth, uint NewHealth) : IEvent;