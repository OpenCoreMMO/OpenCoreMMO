using System.Collections.Generic;
using NeoServer.Game.Common;

namespace NeoServer.Game.Combat.Events;

public static class SharedEvent
{
    private static Dictionary<string, IEvent> _events = new();

    public static TEvent Get<TEvent>() where TEvent : class, IEvent, new()
    {
        var eventName = typeof(TEvent).FullName;

        if (_events.TryGetValue(eventName, out var @event))
        {
            return @event as TEvent;
        }

        @event = new TEvent();
        _events[eventName] = @event;

        return (TEvent)@event;
    }
}