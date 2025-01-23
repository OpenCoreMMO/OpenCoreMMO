using System;
using System.Collections.Generic;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Creatures.Models.Bases.Events;

public static class SharedEvent
{
    private static Dictionary<string, IEvent> _events = new();

    public static TEvent Get<TEvent>() where TEvent : class, IEvent, new()
    {
        var eventName = typeof(IEvent).FullName;

        if (_events.TryGetValue(eventName, out var @event))
        {
            return @event as TEvent;
        }

        @event = new TEvent();
        _events[eventName] = @event;

        return (TEvent)@event;
    }
}

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