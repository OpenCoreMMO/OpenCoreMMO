using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT.Interfaces;

/// <summary>
///     Interface for managing event callbacks.
/// </summary>
public interface IEventsCallbacks
{
    /// <summary>
    ///     Clears all registered event callbacks.
    /// </summary>
    void Clear();

    /// <summary>
    ///     Checks if an event callback is already registered.
    /// </summary>
    /// <param name="callback">The callback to check.</param>
    /// <returns>True if the callback is already registered, otherwise false.</returns>
    bool IsCallbackRegistered(EventCallback callback);

    /// <summary>
    ///     Adds a new event callback to the list.
    /// </summary>
    /// <param name="callback">The callback to add.</param>
    void AddCallback(EventCallback callback);

    /// <summary>
    ///     Executes all callbacks registered for the specified event type.
    /// </summary>
    /// <param name="eventType">The event callback type.</param>
    /// <param name="callbackAction">The action to execute for each callback.</param>
    void ExecuteCallback(EventCallbackType eventType, Action<EventCallback> callbackAction);

    /// <summary>
    ///     Checks if all callbacks registered for the specified event type return true.
    /// </summary>
    /// <param name="eventType">The event callback type.</param>
    /// <param name="callbackFunc">The function to check each callback.</param>
    /// <returns>True if all return true, otherwise false.</returns>
    bool CheckCallback(EventCallbackType eventType, Func<EventCallback, bool> callbackFunc);

    /// <summary>
    ///     Checks all callbacks registered for the specified event type and returns the first value different from
    ///     noErrorValue.
    /// </summary>
    /// <typeparam name="TReturn">The return type of the callback.</typeparam>
    /// <param name="eventType">The event callback type.</param>
    /// <param name="callbackFunc">The function to check each callback.</param>
    /// <param name="noErrorValue">The value considered as "no error".</param>
    /// <returns>The first value different from noErrorValue, or noErrorValue if all return noErrorValue.</returns>
    TReturn CheckCallbackWithReturnValue<TReturn>(EventCallbackType eventType,
        Func<EventCallback, TReturn> callbackFunc, TReturn noErrorValue)
        where TReturn : struct, IEquatable<TReturn>;
}