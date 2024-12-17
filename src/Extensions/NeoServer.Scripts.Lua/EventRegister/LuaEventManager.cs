using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Server.Helpers;
using NLua;
using Serilog;

namespace NeoServer.Scripts.Lua.EventRegister;

/// <summary>
///     A static class for registering Lua scripts to item methods.
/// </summary>
public static class LuaEventManager
{
    private static readonly Dictionary<string, LuaFunction> ItemIdMap = new();
    private static readonly Dictionary<string, LuaFunction> ActionIdMap = new();
    private static readonly Dictionary<string, LuaFunction> UniqueIdMap = new();

    /// <summary>
    ///     Register a Lua function to a given event.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="action">The Lua function to execute when the event is triggered.</param>
    public static void Register(LuaTable eventData, LuaFunction action)
    {
        var logger = IoC.GetInstance<ILogger>();
        var eventName = (string)eventData["event"];

        ushort.TryParse(eventData["serverId"]?.ToString(), out var serverId);
        ushort.TryParse(eventData["actionId"]?.ToString(), out var actionId);
        uint.TryParse(eventData["uniqueId"]?.ToString(), out var uniqueId);

        if (serverId > 0)
        {
            if (!ItemIdMap.TryAdd(ConvertToKey(eventName, serverId), action))
                logger.Warning("Lua action with serverId: {Id} is already registered", serverId);

            return;
        }

        if (uniqueId > 0)
        {
            if (!UniqueIdMap.TryAdd(ConvertToKey(eventName, uniqueId), action))
                logger.Warning("Lua action with uniqueId: {Id} is already registered", uniqueId);

            return;
        }

        if (actionId > 0 && !ActionIdMap.TryAdd(ConvertToKey(eventName, actionId), action))
            logger.Warning("Lua action with actionId: {Id} is already registered", actionId);
    }

    /// <summary>
    ///     Finds the Lua script associated with the specified item and event name.
    /// </summary>
    /// <param name="item">The item to search for the Lua script.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The <see cref="LuaFunction" /> associated with the item and event, or null if not found.</returns>
    public static LuaFunction FindItemScript(IItem item, string eventName)
    {
        eventName = eventName.ToLower();

        if (item.UniqueId > 0 && UniqueIdMap.TryGetValue(ConvertToKey(eventName, item.UniqueId), out var luaFunction))
            return luaFunction;

        if (item.ActionId > 0 && ActionIdMap.TryGetValue(ConvertToKey(eventName, item.ActionId), out luaFunction))
            return luaFunction;

        if (ItemIdMap.TryGetValue(ConvertToKey(eventName, item.ServerId), out luaFunction)) return luaFunction;

        return null;
    }

    private static string ConvertToKey(string eventName, uint id)
    {
        return $"{eventName}{id}".ToLower();
    }
}