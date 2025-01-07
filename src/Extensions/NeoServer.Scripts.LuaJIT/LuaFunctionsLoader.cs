using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LuaNET;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Structs;

namespace NeoServer.Scripts.LuaJIT;

public class LuaFunctionsLoader
{
    public const int LUA_REGISTRY_INDEX = (-10000);
    public const int LUA_ENVIRONMENT_INDEX = (-10001);
    public const int LUA_GLOBALS_INDEX = (-10002);

    public LuaFunctionsLoader()
    {
        //_logger = IoC.GetInstance<ILogger>();

        //_logger.Information("Log from LuaFunctionsLoader");

        for (var i = 0; i < ScriptEnv.Length; i++)
        {
            ScriptEnv[i] = new ScriptEnvironment();
        }
    }

    public static string GetErrorDesc(ErrorCodeType code)
    {
        return code switch
        {
            ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND => "Player not found",
            ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND => "Creature not found",
            ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND => "Npc not found",
            ErrorCodeType.LUA_ERROR_NPC_TYPE_NOT_FOUND => "Npc type not found",
            ErrorCodeType.LUA_ERROR_MONSTER_NOT_FOUND => "Monster not found",
            ErrorCodeType.LUA_ERROR_MONSTER_TYPE_NOT_FOUND => "Monster type not found",
            ErrorCodeType.LUA_ERROR_ITEM_NOT_FOUND => "Item not found",
            ErrorCodeType.LUA_ERROR_THING_NOT_FOUND => "Thing not found",
            ErrorCodeType.LUA_ERROR_TILE_NOT_FOUND => "Tile not found",
            ErrorCodeType.LUA_ERROR_HOUSE_NOT_FOUND => "House not found",
            ErrorCodeType.LUA_ERROR_COMBAT_NOT_FOUND => "Combat not found",
            ErrorCodeType.LUA_ERROR_CONDITION_NOT_FOUND => "Condition not found",
            ErrorCodeType.LUA_ERROR_AREA_NOT_FOUND => "Area not found",
            ErrorCodeType.LUA_ERROR_CONTAINER_NOT_FOUND => "Container not found",
            ErrorCodeType.LUA_ERROR_VARIANT_NOT_FOUND => "Variant not found",
            ErrorCodeType.LUA_ERROR_VARIANT_UNKNOWN => "Unknown variant type",
            ErrorCodeType.LUA_ERROR_SPELL_NOT_FOUND => "Spell not found",
            ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND => "Action not found",
            ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND => "TalkAction not found",
            ErrorCodeType.LUA_ERROR_ZONE_NOT_FOUND => "Zone not found",
            _ => "Bad error code"
        };
    }

    public static int ProtectedCall(LuaState luaState, int nargs, int nresults)
    {
        var ret = 0;
        if (ValidateDispatcherContext(nameof(ProtectedCall)))
        {
            return ret;
        }

        var errorIndex = Lua.GetTop(luaState) - nargs;
        //int errorIndex = -1 - nargs - 1;
        Lua.PushCFunction(luaState, LuaErrorHandler);
        Lua.Insert(luaState, errorIndex);

        //int before = Lua.GetTop(luaState);

        ret = Lua.PCall(luaState, nargs, nresults, errorIndex);
        Lua.Remove(luaState, errorIndex);
        return ret;
    }

    public static void ReportError(string errorDesc) => ReportError("__FUNCTION__", errorDesc);

    public static void ReportError(string function, string errorDesc, bool stackTrace = false)
    {
        GetScriptEnv().GetEventInfo(out var scriptId, out var scriptInterface, out var callbackId, out var timerEvent);

        Console.WriteLine(
            $"Lua script error: \nscriptInterface: [{(scriptInterface != null ? scriptInterface.GetInterfaceName() : "")}]\nscriptId: [{(scriptId != 0 ? scriptInterface?.GetFileById(scriptId) : "")}]\ntimerEvent: [{(timerEvent ? "in a timer event called from:" : "")}]\n callbackId:[{(callbackId != 0 ? scriptInterface?.GetFileById(callbackId) : "")}]\nfunction: [{function ?? ""}]\nerror [{((stackTrace && scriptInterface != null) ? scriptInterface.GetStackTrace(errorDesc) : errorDesc)}]");
    }

    public static int LuaErrorHandler(LuaState luaState)
    {
        var errorMessage = PopString(luaState);
        var scriptInterface = GetScriptEnv().GetScriptInterface();
        Debug.Assert(scriptInterface != null); // This fires if the ScriptEnvironment hasn't been set up
        PushString(luaState, scriptInterface.GetStackTrace(errorMessage));
        return 1;
    }

    public static void PushVariant(LuaState luaState, LuaVariant var)
    {
        if (ValidateDispatcherContext(nameof(PushVariant)))
        {
            return;
        }

        Lua.CreateTable(luaState, 0, 4);
        SetField(luaState, "type", (double)var.Type);

        switch (var.Type)
        {
            case LuaVariantType.VARIANT_NUMBER:
                SetField(luaState, "number", var.Number);
                break;
            case LuaVariantType.VARIANT_STRING:
                SetField(luaState, "string", var.Text);
                break;
            case LuaVariantType.VARIANT_TARGETPOSITION:
            case LuaVariantType.VARIANT_POSITION:
                {
                    PushPosition(luaState, var.Pos);
                    Lua.SetField(luaState, -2, "pos");
                    break;
                }
        }

        SetField(luaState, "instantName", var.InstantName);
        SetField(luaState, "runeName", var.RuneName);
        SetMetatable(luaState, -1, "Variant");
    }

    public static void PushThing(LuaState luaState, IThing? thing)
    {
        if (ValidateDispatcherContext(nameof(PushThing)))
        {
            return;
        }

        switch (thing)
        {
            case null:
                Lua.CreateTable(luaState, 0, 4);
                SetField(luaState, "uid", 0);
                SetField(luaState, "itemid", 0);
                SetField(luaState, "actionid", 0);
                SetField(luaState, "type", 0);
                return;
            case IItem item:
                PushUserdata(luaState, item);
                SetItemMetatable(luaState, -1, item);
                break;
            case ICreature creature:
                PushUserdata(luaState, creature);
                SetCreatureMetatable(luaState, -1, creature);
                break;
            default:
                Lua.PushNil(luaState);
                break;
        }
    }

    public static void PushString(LuaState luaState, string value)
    {
        if (ValidateDispatcherContext(nameof(PushString)))
        {
            return;
        }

        Lua.PushLString(luaState, value, (ulong)value.Length);
    }

    public static void PushCallback(LuaState luaState, int callback)
    {
        if (ValidateDispatcherContext(nameof(PushCallback)))
        {
            return;
        }

        Lua.RawGetI(luaState, LUA_REGISTRY_INDEX, callback);
    }

    public static string PopString(LuaState luaState)
    {
        if (Lua.GetTop(luaState) == 0)
        {
            return string.Empty;
        }

        var str = GetString(luaState, -1);
        Lua.Pop(luaState, 1);
        return str;
    }

    public static int PopCallback(LuaState luaState) => Lua.Ref(luaState, LUA_REGISTRY_INDEX);

    // Metatables
    public static void SetMetatable(LuaState luaState, int index, string name)
    {
        if (ValidateDispatcherContext(nameof(SetMetatable)))
        {
            return;
        }

        Lua.GetMetaTable(luaState, name);
        Lua.SetMetaTable(luaState, index - 1);
    }

    public static void SetWeakMetatable(LuaState luaState, int index, string name)
    {
        var weakObjectTypes = new HashSet<string>();

        if (ValidateDispatcherContext(nameof(SetWeakMetatable)))
        {
            return;
        }

        var weakName = name + "_weak";

        if (weakObjectTypes.Add(name))
        {
            Lua.GetMetaTable(luaState, name);
            var childMetatable = Lua.GetTop(luaState);

            Lua.NewMetaTable(luaState, weakName);
            var metatable = Lua.GetTop(luaState);

            var methodKeys = new List<string> { "__index", "__metatable", "__eq" };
            foreach (var metaKey in methodKeys)
            {
                Lua.GetField(luaState, childMetatable, metaKey);
                Lua.SetField(luaState, metatable, metaKey);
            }

            var methodIndexes = new List<int> { 'h', 'p', 't' };
            foreach (var metaIndex in methodIndexes)
            {
                Lua.RawGetI(luaState, childMetatable, metaIndex);
                Lua.RawSetI(luaState, metatable, metaIndex);
            }

            Lua.PushNil(luaState);
            Lua.SetField(luaState, metatable, "__gc");

            Lua.Remove(luaState, childMetatable);
        }
        else
        {
            Lua.GetMetaTable(luaState, weakName);
        }

        Lua.SetMetaTable(luaState, index - 1);
    }

    public static void SetItemMetatable(LuaState luaState, int index, IItem item)
    {
        if (ValidateDispatcherContext(nameof(SetItemMetatable)))
        {
            return;
        }

        switch (item)
        {
            case { IsContainer: true }:
                Lua.GetMetaTable(luaState, "Container");
                break;
            case { IsTeleport: true }:
                Lua.GetMetaTable(luaState, "Teleport");
                break;
            default:
                Lua.GetMetaTable(luaState, "Item");
                break;
        }

        Lua.SetMetaTable(luaState, index - 1);
    }

    public static void SetCreatureMetatable(LuaState luaState, int index, ICreature creature)
    {
        if (ValidateDispatcherContext(nameof(SetCreatureMetatable)))
        {
            return;
        }

        switch (creature)
        {
            case IPlayer:
                Lua.GetMetaTable(luaState, "Player");
                break;
            case IMonster:
                Lua.GetMetaTable(luaState, "Monster");
                break;
            default:
                Lua.GetMetaTable(luaState, "Npc");
                break;
        }

        Lua.SetMetaTable(luaState, index - 1);
    }

    public static string GetFormatedLoggerMessage(LuaState luaState)
    {
        var format = GetString(luaState, 1);
        var n = Lua.GetTop(luaState);
        var args = new List<object>();

        for (var i = 2; i <= n; i++)
        {
            if (IsString(luaState, i))
            {
                args.Add(Lua.ToString(luaState, i));
            }
            else if (IsNumber(luaState, i))
            {
                args.Add(Lua.ToNumber(luaState, i));
            }
            else if (IsBoolean(luaState, i))
            {
                args.Add(Lua.ToBoolean(luaState, i) ? "true" : "false");
            }
            else if (IsUserdata(luaState, i))
            {
                var userType = GetUserdataType(luaState, i);
                args.Add(GetUserdataTypeName(userType));
            }
            else if (IsTable(luaState, i))
            {
                args.Add("table");
            }
            else if (IsNil(luaState, i))
            {
                args.Add("nil");
            }
            else if (IsFunction(luaState, i))
            {
                args.Add("function");
            }
            else
            {
                Console.WriteLine("[{0}] invalid param type", nameof(GetFormatedLoggerMessage));
            }
        }

        try
        {
            var indexedArguments = args.Select((arg, index) => $"{{{index}}}").ToList();
            var formattedMessage = string.Format(format, indexedArguments.ToArray());
            return formattedMessage;
            //return fmt.vformat(format, args);
        }
        catch (Exception e)
        {
            Console.WriteLine("[{0}] format error: {1}", nameof(GetFormatedLoggerMessage), e.Message);
        }

        return string.Empty;
    }

    public static string GetString(LuaState luaState, int arg)
    {
        ulong len = 0;
        var cStr = Lua.ToLString(luaState, arg, ref len);
        if (cStr == null || len == 0)
        {
            return "";
        }
        return cStr;
    }

    public static Location GetPosition(LuaState luaState, int arg, out int stackpos)
    {
        var position = new Location
        {
            X = GetField<ushort>(luaState, arg, "x"),
            Y = GetField<ushort>(luaState, arg, "y"),
            Z = GetField<byte>(luaState, arg, "z")
        };

        Lua.GetField(luaState, arg, "stackpos");
        stackpos = Lua.IsNil(luaState, -1) ? 0 : GetNumber<int>(luaState, -1);

        Lua.Pop(luaState, 4);
        return position;
    }

    public static Location GetPosition(LuaState luaState, int arg)
    {
        var position = new Location
        {
            X = GetField<ushort>(luaState, arg, "x"),
            Y = GetField<ushort>(luaState, arg, "y"),
            Z = GetField<byte>(luaState, arg, "z")
        };

        Lua.Pop(luaState, 3);
        return position;
    }

    public static LuaVariant GetVariant(LuaState luaState, int arg)
    {
        var var = new LuaVariant
        {
            InstantName = GetFieldString(luaState, arg, "instantName"),
            RuneName = GetFieldString(luaState, arg, "runeName"),
            Type = GetField<LuaVariantType>(luaState, arg, "type")
        };

        switch (var.Type)
        {
            case LuaVariantType.VARIANT_NUMBER:
                var.Number = GetField<uint>(luaState, arg, "number");
                Lua.Pop(luaState, 4);
                break;

            case LuaVariantType.VARIANT_STRING:
                var.Text = GetFieldString(luaState, arg, "string");
                Lua.Pop(luaState, 4);
                break;

            case LuaVariantType.VARIANT_POSITION:
            case LuaVariantType.VARIANT_TARGETPOSITION:
                Lua.GetField(luaState, arg, "pos");
                var.Pos = GetPosition(luaState, Lua.GetTop(luaState));
                break;

            default:
                var.Type = LuaVariantType.VARIANT_NONE;
                Lua.Pop(luaState, 3);
                break;
        }

        return var;
    }

    public static string GetFieldString(LuaState luaState, int arg, string key)
    {
        Lua.GetField(luaState, arg, key);
        return GetString(luaState, -1);
    }

    public static LuaDataType GetUserdataType(LuaState luaState, int arg)
    {
        if (Lua.GetMetaTable(luaState, arg) == 0)
        {
            return LuaDataType.Unknown;
        }
        Lua.RawGetI(luaState, -1, 't');

        var type = GetNumber<LuaDataType>(luaState, -1);
        Lua.Pop(luaState, 2);

        return type;
    }

    public static string GetUserdataTypeName(LuaDataType userType)
    {
        //return magic_enum::enum_name(userType).data();
        return userType.ToString();
    }

    // Push
    public static void PushBoolean(LuaState luaState, bool value)
    {
        if (ValidateDispatcherContext(nameof(PushBoolean)))
        {
            return;
        }

        Lua.PushBoolean(luaState, value);
    }

    public static void PushPosition(LuaState luaState, Location position, int stackpos = 0)
    {
        if (ValidateDispatcherContext(nameof(PushPosition)))
        {
            return;
        }

        Lua.CreateTable(luaState, 0, 4);

        SetField(luaState, "x", position.X);
        SetField(luaState, "y", position.Y);
        SetField(luaState, "z", position.Z);
        SetField(luaState, "stackpos", stackpos);

        SetMetatable(luaState, -1, "Position");
    }

    public static void RegisterClass(LuaState luaState, string className, string baseClass, LuaFunction newFunction = null)
    {
        // className = {}
        Lua.NewTable(luaState);
        Lua.PushValue(luaState, -1);
        Lua.SetGlobal(luaState, className);
        var methods = Lua.GetTop(luaState);

        // methodsTable = {}
        Lua.NewTable(luaState);
        var methodsTable = Lua.GetTop(luaState);

        if (newFunction != null)
        {
            // className.__call = newFunction
            Lua.PushCFunction(luaState, newFunction);
            Lua.SetField(luaState, methodsTable, "__call");
        }

        uint parents = 0;
        if (!string.IsNullOrEmpty(baseClass))
        {
            Lua.GetGlobal(luaState, baseClass);
            Lua.RawGetI(luaState, -1, 'p');
            parents = GetNumber<uint>(luaState, -1) + 1;
            Lua.Pop(luaState, 1);
            Lua.SetField(luaState, methodsTable, "__index");
        }

        // setmetatable(className, methodsTable)
        Lua.SetMetaTable(luaState, methods);

        // className.metatable = {}
        Lua.NewMetaTable(luaState, className);
        var metatable = Lua.GetTop(luaState);

        // className.metatable.__metatable = className
        Lua.PushValue(luaState, methods);
        Lua.SetField(luaState, metatable, "__metatable");

        // className.metatable.__index = className
        Lua.PushValue(luaState, methods);
        Lua.SetField(luaState, metatable, "__index");

        // className.metatable['h'] = hash
        Lua.PushNumber(luaState, (double)className.GetHashCode());
        Lua.RawSetI(luaState, metatable, 'h');

        // className.metatable['p'] = parents
        Lua.PushNumber(luaState, parents);
        Lua.RawSetI(luaState, metatable, 'p');

        // className.metatable['t'] = type
        Enum.TryParse<LuaDataType>(className, true, out var userTypeEnum);

        Lua.PushNumber(luaState, (double)userTypeEnum);

        Lua.RawSetI(luaState, metatable, 't');

        // pop className, className.metatable
        Lua.Pop(luaState, 2);
    }

    public static void RegisterMethod(LuaState luaState, string globalName, string methodName, LuaFunction func)
    {
        // globalName.methodName = func
        Lua.GetGlobal(luaState, globalName);
        Lua.PushCFunction(luaState, func);
        Lua.SetField(luaState, -2, methodName);

        // pop globalName
        Lua.Pop(luaState, 1);
    }

    public static void RegisterTable(LuaState luaState, string tableName)
    {
        // _G[tableName] = {}
        Lua.NewTable(luaState);
        Lua.SetGlobal(luaState, tableName);
    }

    public static void RegisterMetaMethod(LuaState luaState, string className, string methodName, LuaFunction func)
    {
        // className.metatable.methodName = func
        Lua.GetMetaTable(luaState, className);
        Lua.PushCFunction(luaState, func);
        Lua.SetField(luaState, -2, methodName);

        // pop className.metatable
        Lua.Pop(luaState, 1);
    }

    public static void RegisterVariable(LuaState luaState, string tableName, string name, double value)
    {
        // tableName.name = value
        Lua.GetGlobal(luaState, tableName);
        SetField(luaState, name, value);

        // pop tableName
        Lua.Pop(luaState, 1);
    }

    public static void RegisterVariable(LuaState luaState, string tableName, string name, string value)
    {
        // tableName.name = value
        Lua.GetGlobal(luaState, tableName);
        SetField(luaState, name, value);

        // pop tableName
        Lua.Pop(luaState, 1);
    }
    public static void RegisterVariable(LuaState luaState, string tableName, string name, BooleanConfigType value)
    {
        RegisterVariable(luaState, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState luaState, string tableName, string name, StringConfigType value)
    {
        RegisterVariable(luaState, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState luaState, string tableName, string name, IntegerConfigType value)
    {
        RegisterVariable(luaState, tableName, name, (double)value);
    }

    public static void RegisterVariable(LuaState luaState, string tableName, string name, FloatingConfigType value)
    {
        RegisterVariable(luaState, tableName, name, (double)value);
    }

    public static void RegisterGlobalBoolean(LuaState luaState, string name, bool value)
    {
        // _G[name] = value
        PushBoolean(luaState, value);
        Lua.SetGlobal(luaState, name);
    }

    public static void RegisterGlobalMethod(LuaState luaState, string functionName, LuaFunction func)
    {
        // _G[functionName] = func
        Lua.PushCFunction(luaState, func);
        Lua.SetGlobal(luaState, functionName);
    }

    public static void RegisterGlobalVariable(LuaState luaState, string name, double value)
    {
        // _G[name] = value
        Lua.PushNumber(luaState, value);
        Lua.SetGlobal(luaState, name);
    }

    public static void RegisterGlobalVariable(LuaState luaState, string name, ReloadType value)
    {
        // _G[name] = value
        RegisterGlobalVariable(luaState, name, (double)value);
    }

    public static void RegisterGlobalVariable(LuaState luaState, string name, MessageClassesType value)
    {
        // _G[name] = value
        RegisterGlobalVariable(luaState, name, (double)value);
    }

    public static void RegisterGlobalString(LuaState luaState, string variable, string name)
    {
        // Example: RegisterGlobalString(luaState, "VARIABLE_NAME", "variable string");
        PushString(luaState, name);
        Lua.SetGlobal(luaState, variable);
    }

    public static string EscapeString(string str)
        => str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'")
            .Replace("[[", "\\[[");

    public static int LuaUserdataCompare<T>(LuaState luaState) where T : class
    {
        PushBoolean(luaState, GetUserdata<T>(luaState, 1) == GetUserdata<T>(luaState, 2));
        return 1;
    }

    public static int LuaUserdataCompareStruct<T>(LuaState luaState) where T : struct
    {
        PushBoolean(luaState, EqualityComparer<T>.Default.Equals(GetUserdataStruct<T>(luaState, 1), GetUserdataStruct<T>(luaState, 2)));
        return 1;
    }

    public static void RegisterSharedClass(LuaState luaState, string className, string baseClass, LuaFunction newFunction)
    {
        RegisterClass(luaState, className, baseClass, newFunction);
        RegisterMetaMethod(luaState, className, "__gc", LuaGarbageCollection);
    }

    public static int LuaGarbageCollection(LuaState luaState)
    {
        try
        {
            //const auto objPtr = static_cast<std::shared_ptr<SharedObject>*>(lua_touserdata(luaState, 1));
            //if (objPtr)
            //{
            //    objPtr->reset();
            //}
            //return 0;
        }
        catch (Exception e)
        {
            //Logger.GetInstance().Error($"Exception in GarbageCollection: {e.InnerException}");
        }

        return 0;
    }

    public static bool ValidateDispatcherContext(string fncName)
    {
        //if (g_dispatcher().context().isOn() && g_dispatcher().context().isAsync())
        //{
        //    g_logger().warn("[{}] The call to lua was ignored because the '{}' task is trying to communicate while in async mode.", fncName, g_dispatcher().context().getName());
        //    return LUA_ERRRUN > 0;
        //}

        //return 0;

        return false;
    }

    private static int _scriptEnvIndex;
    private static readonly ScriptEnvironment[] ScriptEnv = new ScriptEnvironment[16];

    public static T GetNumber<T>(LuaState luaState, int arg) where T : struct
    {
        if (typeof(T).IsEnum)
        {
            return (T)Enum.ToObject(typeof(T), (long)Lua.ToNumber(luaState, arg));
        }
        else if (typeof(T).IsPrimitive)
        {
            return (T)Convert.ChangeType(Lua.ToNumber(luaState, arg), typeof(T));
        }
        else
        {
            throw new NotSupportedException($"Type {typeof(T)} is not supported.");
        }
    }

    public static T GetNumber<T>(LuaState luaState, int arg, T defaultValue) where T : struct
    {
        var parameters = Lua.GetTop(luaState);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return GetNumber<T>(luaState, arg);
    }

    public static T GetUserdataShared<T>(LuaState luaState, int arg) where T : struct
    {
        var userdata = (IntPtr)Lua.ToUserData(luaState, arg);

        if (userdata == IntPtr.Zero)
        {
            return default(T);
        }

        var ptr = Marshal.ReadIntPtr(userdata);
        return Marshal.PtrToStructure<T>(ptr)!;
    }

    public static T GetUserdata<T>(LuaState luaState, int arg) where T : class
    {
        var userdata = GetRawUserdata<T>(luaState, arg);
        if (userdata == IntPtr.Zero)
        {
            return null;
        }

        var structure = Marshal.PtrToStructure<UserDataStruct>(Marshal.ReadIntPtr(userdata))!;

        lock (Objects)
        {
            if (Objects.TryGetValue(structure.Index, out var value))
                return (T)value;
        }

        return null;
    }

    public static T GetUserdataStruct<T>(LuaState luaState, int arg) where T : struct
    {
        var userdata = GetRawUserdataStruct<T>(luaState, arg);
        if (userdata == IntPtr.Zero)
        {
            return default;
        }

        var structure = Marshal.PtrToStructure<UserDataStruct>(Marshal.ReadIntPtr(userdata));
        
        lock (Objects)
        {
            if (Objects.TryGetValue(structure.Index, out var value))
                return (T)value;
        }

        return default;
    }

    public static IntPtr GetRawUserdata<T>(LuaState luaState, int arg) where T : class
    {
        return (nint)Lua.ToUserData(luaState, arg);
    }

    public static IntPtr GetRawUserdataStruct<T>(LuaState luaState, int arg) where T : struct
    {
        return (nint)Lua.ToUserData(luaState, arg);
    }

    public static bool GetBoolean(LuaState luaState, int arg)
    {
        return Lua.ToBoolean(luaState, arg);
    }

    public static bool GetBoolean(LuaState luaState, int arg, bool defaultValue)
    {
        var parameters = Lua.GetTop(luaState);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return Lua.ToBoolean(luaState, arg);
    }

    public static string GetString(LuaState luaState, int arg, string defaultValue)
    {
        var parameters = Lua.GetTop(luaState);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return GetString(luaState, arg);
    }

    public static T GetField<T>(LuaState luaState, int arg, string key) where T : struct
    {
        Lua.GetField(luaState, arg, key);
        return GetNumber<T>(luaState, -1);
    }

    public static bool IsNumber(LuaState luaState, int arg)
    {
        return Lua.Type(luaState, arg) == LuaNET.LuaType.Number;
    }

    public static bool IsString(LuaState luaState, int arg)
    {
        return Lua.IsString(luaState, arg);
    }

    public static bool IsBoolean(LuaState luaState, int arg)
    {
        return Lua.IsBoolean(luaState, arg);
    }

    public static bool IsTable(LuaState luaState, int arg)
    {
        return Lua.IsTable(luaState, arg);
    }

    public static bool IsFunction(LuaState luaState, int arg)
    {
        return Lua.IsFunction(luaState, arg);
    }

    public static bool IsNil(LuaState luaState, int arg)
    {
        return Lua.IsNil(luaState, arg);
    }

    public static bool IsUserdata(LuaState luaState, int arg)
    {
        return Lua.IsUserData(luaState, arg);
    }

    public static void SetField(LuaState luaState, string index, double value)
    {
        Lua.PushNumber(luaState, value);
        Lua.SetField(luaState, -2, index);
    }

    public static void SetField(LuaState luaState, string index, string value)
    {
        PushString(luaState, value);
        Lua.SetField(luaState, -2, index);
    }

    public ScriptEnvironment InternalGetScriptEnv()
    {
        if (_scriptEnvIndex is < 0 or >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return ScriptEnv[_scriptEnvIndex];
    }

    public bool InternalReserveScriptEnv() => ++_scriptEnvIndex < 16;

    public static ScriptEnvironment GetScriptEnv()
    {
        if (_scriptEnvIndex is < 0 or >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return ScriptEnv[_scriptEnvIndex];
    }

    public static bool ReserveScriptEnv()
    {
        return ++_scriptEnvIndex < 16;
    }

    public static void ResetScriptEnv()
    {
        if (_scriptEnvIndex < 0)
        {
            throw new IndexOutOfRangeException();
        }
        ScriptEnv[_scriptEnvIndex--].ResetEnv();
    }

    /// <summary>
    /// Compatibility NewIndexedUserData with constant parameter
    /// </summary>
    /// <returns></returns>
    public static IntPtr NewUserData(LuaState luaState, int size)
    {
        return Lua.NewUserData(luaState, (UIntPtr)size);
    }

    public static void NewUData(LuaState luaState, int val)
    {
        var pointer = NewUserData(luaState, Marshal.SizeOf<int>());
        Marshal.WriteInt32(pointer, val);
    }

    public static T ToObject<T>(LuaState luaState, int index, bool freeGcHandle = true)
    {
        if (IsNil(luaState, index)/* || !IsLightUserData(index)*/)
            return default;

        var data = Lua.ToUserData(luaState, index);
        if (data == IntPtr.Zero)
            return default;

        var handle = GCHandle.FromIntPtr(data);
        if (!handle.IsAllocated)
            return default;

        var reference = (T)handle.Target;

        if (freeGcHandle)
            handle.Free();

        return reference;
    }

    // Compare cache entries by exact reference to avoid unwanted aliases
    private class ReferenceComparer : IEqualityComparer<object>
    {
        public new bool Equals(object x, object y)
        {
            if (x != null && y != null && x.GetType() == y.GetType() && x.GetType().IsValueType && y.GetType().IsValueType)
                return x.Equals(y); // Special case for boxed value types
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    // object to object #
    private static readonly ConcurrentDictionary<object, int> ObjectsBackMap = new(new ReferenceComparer());
    // object # to object (FIXME - it should be possible to get object address as an object #)
    private static readonly ConcurrentDictionary<int, object> Objects = new();
    private static readonly List<UserDataStruct> StructuresToReuse = [];

    private static readonly ConcurrentQueue<int> FinalizedReferences = new();

    //internal EventHandlerContainer PendingEvents = new EventHandlerContainer();

    /// <summary>
    /// We want to ensure that objects always have a unique ID
    /// </summary>
    private static int _nextObj;

    private static int AddObject(object obj)
    {
        // New object: inserts it in the list
        var index = _nextObj++;

        lock (Objects)
        {
            Objects[index] = obj;

            if (!obj.GetType().IsValueType || obj.GetType().IsEnum)
                ObjectsBackMap[obj] = index;
        }
       
        return index;
    }

    public static void PushUserdata(LuaState luaState, object o)
    {
        // Pushes nil
        if (o == null)
        {
            Lua.PushNil(luaState);
            return;
        }

        // Object already in the list of Lua objects? Push the stored reference.
        var found = (!o.GetType().IsValueType || o.GetType().IsEnum) && ObjectsBackMap.TryGetValue(o, out var index);

        index = AddObject(o);

        IntPtr userdata;

        UserDataStruct structure;

        if (StructuresToReuse.Count != 0)
        {
            structure = StructuresToReuse.FirstOrDefault();
            StructuresToReuse.Remove(structure);

            structure.Index = index;
            userdata = structure.Ptr;
        }
        else
        {
            userdata = Lua.NewUserData(luaState, (ulong)IntPtr.Size);
            structure = new UserDataStruct(index, userdata, 0);
        }

        var gch2 = GCHandle.Alloc(structure, GCHandleType.Pinned);

        Marshal.WriteIntPtr(userdata, gch2.AddrOfPinnedObject());
        gch2.Free();
    }

    public static void UpdateLuaUserdata(LuaState luaState, int index, IItem newItem)
    {
        var userdata = Lua.ToUserData(luaState, index);
        if (userdata == IntPtr.Zero)
            throw new Exception("Failed to update userdata: userdata not found.");

        var userDataStruct = Marshal.PtrToStructure<UserDataStruct>(Marshal.ReadIntPtr(userdata))!;

        lock (Objects)
            if (Objects.ContainsKey(userDataStruct.Index))
                Objects[userDataStruct.Index] = newItem;
    }
}
