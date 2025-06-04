using NeoServer.Domain.Common;

namespace NeoServer.Domain.Combat.Events;

public static class SharedEvent
{
    private static readonly Dictionary<string, IEvent> _events = new();

    public static TEvent Get<TEvent>() where TEvent : class, IEvent, new()
    {
        var eventName = typeof(TEvent).FullName;

        if (_events.TryGetValue(eventName, out var @event)) return @event as TEvent;

        @event = new TEvent();
        _events[eventName] = @event;

        return (TEvent)@event;
    }
}