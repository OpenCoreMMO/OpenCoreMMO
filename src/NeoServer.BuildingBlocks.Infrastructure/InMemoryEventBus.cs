using NeoServer.Game.Common.Helpers;

namespace NeoServer.BuildingBlocks.Infrastructure;
public class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly Dictionary<string, List<IIntegrationEventHandler>> _handlers = new();

    public void Initialize()
    {
        var handlersGroup = GameAssemblyCache
            .Cache
            .Where(type => typeof(IIntegrationEventHandler).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface)
            .GroupBy(type => type.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                .GetGenericArguments().First().FullName);

        foreach (var group in handlersGroup)
        {
            var eventNane = group.Key;
            if (eventNane is null) continue;

            var handlers = group.ToList();

            _handlers[eventNane] =
                handlers.Select(x => (IIntegrationEventHandler)serviceProvider.GetService(x))
                    .ToList();
        }
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        var eventName = @event?.GetType().FullName;
        if (string.IsNullOrWhiteSpace(eventName)) return;

        if (!_handlers.TryGetValue(eventName, out var handlers))
        {
            return;
        }

        foreach (var eventHandler in handlers)
        {
            if (eventHandler is IIntegrationEventHandler<TEvent> handler)
            {
                handler.Handle(@event);
            }
        }
    }
}

public interface IIntegrationEvent;
public interface IIntegrationEventHandler;
public interface IIntegrationEventHandler<in T> : IIntegrationEventHandler where T : IIntegrationEvent
{
    void Handle(T @event);
}

public interface IEventBus
{
    void Initialize();
    void Publish<TEvent>(TEvent @event) where TEvent : IIntegrationEvent;
}

