using System.Reflection;
using NeoServer.Domain.Common;

namespace NeoServer.Domain.Tests.Helpers;

public static class EventAggregatorTestHelper
{
    public static EventAggregator SetupEventAggregator<TEvent>(Action<TEvent> onEvent) where TEvent : IEvent
    {
        var aggregator = GetOrCreateAggregator();

        var handlersField = typeof(EventAggregator).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Instance);
        var handlers = (Dictionary<string, List<Action<IEvent>>>)handlersField?.GetValue(aggregator);
        handlers[typeof(TEvent).FullName] = new List<Action<IEvent>> { e => onEvent((TEvent)e) };

        return aggregator;
    }

    private static EventAggregator GetOrCreateAggregator()
    {
        var instanceField = typeof(EventAggregator).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
        var existing = instanceField?.GetValue(null) as EventAggregator;
        if (existing != null) return existing;

        var sp = new TestServiceProvider();
        var aggregator = new EventAggregator(sp);
        instanceField?.SetValue(null, aggregator);
        return aggregator;
    }

    private class TestServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return null;
        }
    }
}
