using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class EventCallbackEntry
{
    public EventCallbackEntry(string name, EventCallback callback)
    {
        Name = name;
        Callback = callback;
    }

    public string Name { get; }
    public EventCallback Callback { get; }
}

public class EventsCallbacks : IEventsCallbacks
{
    private static readonly Lazy<EventsCallbacks> _instance = new(() => new EventsCallbacks());

    private readonly Dictionary<EventCallbackType, List<EventCallbackEntry>> _callbacks = new();

    private readonly ILogger _logger;

    public EventsCallbacks()
    {
        // Use your IoC or logger instance as needed
        _logger = Log.Logger;
    }

    public static EventsCallbacks Instance => _instance.Value;

    public bool IsCallbackRegistered(EventCallback callback)
    {
        if (!_callbacks.TryGetValue(callback.EventType, out var callbackList))
            return false;

        var found = callbackList.Any(entry => entry.Name == callback.CallbackName);
        return found && !callback.SkipDuplicationCheck;
    }

    public void AddCallback(EventCallback callback)
    {
        if (!_callbacks.TryGetValue(callback.EventType, out var callbackList))
        {
            callbackList = new List<EventCallbackEntry>();
            _callbacks[callback.EventType] = callbackList;
        }

        if (callbackList.Any(entry => entry.Name == callback.CallbackName && !callback.SkipDuplicationCheck))
        {
            _logger?.Debug("Event callback already registered: {CallbackName}", callback.CallbackName);
            return;
        }

        _logger?.Debug("Registering event callback: {CallbackName}", callback.CallbackName);
        callbackList.Add(new EventCallbackEntry(callback.CallbackName, callback));
    }

    public void Clear()
    {
        _callbacks.Clear();
    }

    // Execute all callbacks of a given type with a provided action
    public void ExecuteCallback(EventCallbackType eventType, Action<EventCallback> callbackAction)
    {
        if (!_callbacks.TryGetValue(eventType, out var callbackList))
            return;

        foreach (var entry in callbackList)
            if (entry.Callback != null && entry.Callback.IsLoadedScriptId())
                callbackAction(entry.Callback);
    }

    // Check if all callbacks of a given type succeed (return true)
    public bool CheckCallback(EventCallbackType eventType, Func<EventCallback, bool> callbackFunc)
    {
        if (!_callbacks.TryGetValue(eventType, out var callbackList))
            return true;

        var allSucceeded = true;
        foreach (var entry in callbackList)
            if (entry.Callback != null && entry.Callback.IsLoadedScriptId())
                allSucceeded &= callbackFunc(entry.Callback);

        return allSucceeded;
    }

    // Check with return value (for enums, e.g., ReturnValue)
    public TReturn CheckCallbackWithReturnValue<TReturn>(EventCallbackType eventType,
        Func<EventCallback, TReturn> callbackFunc, TReturn noErrorValue)
        where TReturn : struct, IEquatable<TReturn>
    {
        if (!_callbacks.TryGetValue(eventType, out var callbackList))
            return noErrorValue;

        foreach (var entry in callbackList)
            if (entry.Callback != null && entry.Callback.IsLoadedScriptId())
            {
                var result = callbackFunc(entry.Callback);
                if (!result.Equals(noErrorValue))
                    return result;
            }

        return noErrorValue;
    }
}