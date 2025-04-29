using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public record CreatureConditionAddedEvent(ICreature Creature, ICondition Condition) : IEvent;
public record CreatureConditionRemovedEvent(ICreature Creature, ICondition Condition) : IEvent;