using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Creatures.Models.Bases.Events;

public record CreatureConditionAddedEvent(ICreature Creature, ICondition Condition) : IEvent;

public record CreatureConditionRemovedEvent(ICreature Creature, ICondition Condition) : IEvent;