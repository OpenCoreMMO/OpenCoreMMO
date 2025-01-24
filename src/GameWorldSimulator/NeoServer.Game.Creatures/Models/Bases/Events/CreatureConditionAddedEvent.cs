using NeoServer.Game.Combat.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public class CreatureConditionAddedEvent : IEvent
{
    public ICreature Creature { get; private set; }
    public ICondition Condition { get; private set; }

    public static CreatureConditionAddedEvent SetValues(ICreature creature, ICondition condition)
    {
        var instance = SharedEvent.Get<CreatureConditionAddedEvent>();
        instance.Creature = creature;
        instance.Condition = condition;
        return instance;
    }
}