using System.Reflection;
using NeoServer.Domain.Common;

namespace NeoServer.Domain.Tests.Helpers;

public static class EventAggregatorTestHelper
{
    public static EventAggregator SetupEventAggregator<TEvent>(Action<TEvent> onEvent) where TEvent : IEvent
    {
        var sp = new TestServiceProvider();
        var aggregator = new EventAggregator(sp);

        typeof(EventAggregator).GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static)
            ?.SetValue(null, aggregator);

        var handlersField = typeof(EventAggregator).GetField("_handlers", BindingFlags.NonPublic | BindingFlags.Instance);
        var handlers = (Dictionary<string, List<Action<IEvent>>>)handlersField?.GetValue(aggregator);
        handlers[typeof(TEvent).FullName] = new List<Action<IEvent>> { e => onEvent((TEvent)e) };

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
