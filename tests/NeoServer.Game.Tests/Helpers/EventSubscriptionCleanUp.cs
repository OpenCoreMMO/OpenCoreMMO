using System;
using System.Reflection;

namespace NeoServer.Game.Tests.Helpers;

public  static class EventSubscriptionCleanUp
{
    public static void CleanUp<T>(string eventName)
    {
        var type = typeof(T);
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException(nameof(eventName));

        // Get the event info from the type
        var eventInfo = type.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (eventInfo == null)
            throw new ArgumentException($"Event '{eventName}' not found on type '{type.Name}'");

        // Get the field backing the event
        var fieldInfo = type.GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static);
        if (fieldInfo == null)
            throw new InvalidOperationException($"Event '{eventName}' does not have a backing field.");

        // Get the delegate stored in the field
        var eventDelegate = fieldInfo.GetValue(null) as Delegate;
        if (eventDelegate == null) return;

        // Unsubscribe all handlers
        foreach (var handler in eventDelegate.GetInvocationList())
        {
            eventInfo.RemoveEventHandler(null, handler);
        }
    }
}