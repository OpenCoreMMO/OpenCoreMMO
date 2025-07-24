using LuaNET;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class EventCallbackFunctions : LuaScriptInterface, IEventCallbackFunctions
{
    private static ILogger _logger;
    private static IEventsCallbacks _eventsCallbacks;
    private static IScripts _scripts;

    public EventCallbackFunctions(
        ILogger logger,
        IEventsCallbacks eventsCallbacks,
        IScripts scripts,
        IVocationStore vocationStore) : base(nameof(EventCallbackFunctions))
    {
        _logger = logger;
        _eventsCallbacks = eventsCallbacks;
        _scripts = scripts;
    }

    public void Init(LuaState L)
    {
        RegisterSharedClass(L, "EventCallback", "", LuaCreateEventCallback);
        RegisterMethod(L, "EventCallback", "type", LuaEventCallbackType);
        RegisterMethod(L, "EventCallback", "register", LuaEventCallbackRegister);
    }

    private static int LuaCreateEventCallback(LuaState L)
    {
        var callbackName = GetString(L, 2);
        if (string.IsNullOrEmpty(callbackName))
        {
            ReportError("Invalid callback name");
            return 1;
        }

        bool skipDuplicationCheck = GetBoolean(L, 3, false);

        var eventCallback = new EventCallback(GetScriptEnv().GetScriptInterface(), _logger, _scripts);

        eventCallback.CallbackName = callbackName;
        eventCallback.SkipDuplicationCheck = skipDuplicationCheck;

        PushUserdata(L, eventCallback);
        SetMetatable(L, -1, "EventCallback");
        return 1;
    }

    public static int LuaEventCallbackType(LuaState L)
    {
        // EventCallback:type(typeName)
        var callback = GetUserdata<EventCallback>(L, 1);
        if (callback == null)
        {
            ReportError("EventCallback is nil");
            return 0;
        }

        var typeName = GetString(L, 2);
        if (string.IsNullOrEmpty(typeName))
        {
            ReportError("EventCallback type name is empty");
            PushBoolean(L, false);
            return 1;
        }

        var lowerTypeName = typeName.ToLowerInvariant();
        var found = false;

        foreach (var enumValue in Enum.GetValues<EventCallbackType>())
        {
            var enumName = enumValue.ToString();
            if (enumName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            {
                callback.EventType = enumValue;
                callback.ScriptTypeName = typeName;

                found = true;
                break;
            }
        }

        if (!found)
        {
            _logger?.Error("[LuaEventCallbackType] No valid event name: {TypeName}", typeName);
            PushBoolean(L, false);
            return 1;
        }

        PushBoolean(L, true);
        return 1;
    }

    public static int LuaEventCallbackRegister(LuaState L)
    {
        // EventCallback:register()
        var callback = GetUserdata<EventCallback>(L, 1);
        if (callback == null)
            return 0;

        if (!callback.IsLoadedScriptId())
            return 0;

        if (_eventsCallbacks.IsCallbackRegistered(callback))
        {
            ReportError($"EventCallback is duplicated for event with name: {callback.CallbackName}");
            return 0;
        }

        _eventsCallbacks.AddCallback(callback);
        PushBoolean(L, true);
        return 1;
    }

    public static int LuaEventCallbackLoad(LuaState L)
    {
        // EventCallback:load()
        var callback = GetUserdata<EventCallback>(L, 1);
        if (callback == null)
            return 1;

        if (!callback.LoadScriptId())
        {
            ReportError("Cannot load callback");
            return 1;
        }

        PushBoolean(L, true);
        return 1;
    }
}