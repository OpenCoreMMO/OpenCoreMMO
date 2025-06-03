using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Common;

public class EventAggregator : IEventAggregator
{
    private static readonly List<(Action<IEvent> Handler, IEvent Event)> DeferredHandlersCache = new(100);

    private readonly Queue<IEvent> _eventQueue = new();
    private readonly Dictionary<string, List<Action<IEvent>>> _handlers = new();
    private readonly Dictionary<string, List<Action<IEvent>>> _networkHandlers = new();
    private readonly IServiceProvider _serviceProvider;

    public EventAggregator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Instance = this;
    }

    public static IEventAggregator Instance { get; private set; }

    public void Initialize()
    {
        var handlersGroup = GameAssemblyCache
            .Cache
            .Where(type => typeof(IApplicationEventHandler).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface)
            .SelectMany(type =>
                type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IApplicationEventHandler<>))
                    .Select(i => new
                    {
                        EventTypeFullName = i.GetGenericArguments().First().FullName,
                        HandlerType = type
                    }))
            .GroupBy(x => x.EventTypeFullName, x => x.HandlerType);

        foreach (var group in handlersGroup)
        {
            var eventNane = group.Key;
            if (eventNane is null) continue;

            var handlers = group.ToList();

            _handlers[eventNane] =
                handlers.Select(x =>
                    {
                        var handlerInstance = _serviceProvider.GetService(x);
                        IApplicationEventHandler<IEvent> handler;

                        if (x.GetInterfaces()
                            .Any(i => i.IsGenericType &&
                                      i.GetGenericTypeDefinition() == typeof(INetworkingEventHandler<>)))
                            return null;

                        var handleMethod = x.GetMethod(nameof(handler.Handle));
                        Action<IEvent> handlerDelegate = @event => handleMethod.Invoke(handlerInstance, [@event]);
                        return handlerDelegate;
                    })
                    .Where(x => x is not null)
                    .ToList();

            _networkHandlers[eventNane] =
                handlers.Select(x =>
                    {
                        var handlerInstance = _serviceProvider.GetService(x);

                        if (!x.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().FullName ==
                                    typeof(INetworkingEventHandler<>).FullName))
                            return null;

                        var handleMethod = x.GetMethods()
                            .FirstOrDefault(m =>
                                m.Name == nameof(INetworkingEventHandler<IEvent>.Handle) &&
                                m.GetParameters().Length == 1 &&
                                m.GetParameters()[0].ParameterType.FullName == eventNane);

                        Action<IEvent> handlerDelegate = @event => handleMethod.Invoke(handlerInstance, [@event]);
                        return handlerDelegate;
                    })
                    .Where(x => x is not null)
                    .ToList();
        }
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        _eventQueue.Enqueue(@event);
    }

    public void PropagateEvents()
    {
        if (_eventQueue.Count == 0) return;

        // Queue for other handlers to ensure they run after network handlers
        while (_eventQueue.TryDequeue(out var @event))
        {
            var eventName = @event?.GetType().FullName;
            if (string.IsNullOrWhiteSpace(eventName)) return;

            // Process network handlers first
            if (_networkHandlers.TryGetValue(eventName, out var networkHandlers))
                foreach (var networkHandler in networkHandlers)
                    networkHandler?.Invoke(@event);

            // Collect other handlers to execute later
            if (_handlers.TryGetValue(eventName, out var otherHandlers))
                foreach (var otherHandler in otherHandlers)
                    DeferredHandlersCache.Add((otherHandler, @event));
        }

        if (DeferredHandlersCache.Count == 0) return;

        // Execute deferred handlers
        foreach (var (handler, @event) in DeferredHandlersCache) handler?.Invoke(@event);

        DeferredHandlersCache.Clear();
    }

    public static void Publish(IEvent @event)
    {
        Instance?.Publish(@event);
    }
}

public interface IEvent;

//todo: rename to IEventHandler when all old events are removed
public interface IApplicationEventHandler;

public interface IApplicationEventHandler<in T> : IApplicationEventHandler where T : IEvent
{
    void Handle(T @event);
}

public interface INetworkingEventHandler<in T> : IApplicationEventHandler<T> where T : IEvent;

public interface IEventAggregator
{
    void Initialize();
    void Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    void PropagateEvents();
}