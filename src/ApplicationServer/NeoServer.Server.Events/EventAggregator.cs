using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.ObjectPool;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Server.Events;

public interface IBaseEvent
{
    void Reset();
}

public interface IBaseEventHandler { }

public interface IBaseEventHandler<in T> : IBaseEventHandler where T : IBaseEvent
{
    void Handle(T @event);
}

public class EventAggregator
{
    private static EventAggregator _instance = null;
    public static EventAggregator Instance => _instance ??= new EventAggregator();

    private readonly Dictionary<string, List<IBaseEventHandler>> _handlers = new();
    private readonly Dictionary<Type, object> _eventPools = new();

    public void Initialize(IServiceProvider serviceProvider)
    {
        var handlersGroup = GameAssemblyCache
            .Cache
            .Where(type => typeof(IBaseEventHandler).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface)
            .GroupBy(type => type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBaseEventHandler<>))
                .GetGenericArguments().First().FullName);

        foreach (var group in handlersGroup)
        {
            var eventName = group.Key;
            if (eventName is null) continue;

            var handlers = group.ToList();

            _handlers[eventName] =
                handlers.Select(x => (IBaseEventHandler)serviceProvider.GetService(x))
                    .ToList();
        }
    }

    public void Publish<TEvent>(Action<TEvent> configureEvent) where TEvent : class, IBaseEvent, new()
    {
        var eventType = typeof(TEvent);
        if (!_eventPools.TryGetValue(eventType, out var pool))
        {
            pool = new DefaultObjectPool<TEvent>(new DefaultPooledObjectPolicy<TEvent>());
            _eventPools[eventType] = pool;
        }

        var eventPool = (ObjectPool<TEvent>)pool;
        var @event = eventPool.Get();

        try
        {
            configureEvent(@event);

            var eventName = eventType.FullName;
            if (string.IsNullOrWhiteSpace(eventName)) return;

            if (!_handlers.TryGetValue(eventName, out var handlers))
                return;

            foreach (var eventHandler in handlers)
                if (eventHandler is IBaseEventHandler<TEvent> handler)
                    handler.Handle(@event);
        }
        finally
        {
            if (@event is IBaseEvent resettableEvent)
                resettableEvent.Reset();

            eventPool.Return(@event);
        }
    }
}