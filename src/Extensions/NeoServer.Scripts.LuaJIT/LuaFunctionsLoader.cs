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
    public const int LUA_REGISTRYINDEX = (-10000);
    public const int LUA_ENVIRONINDEX = (-10001);
    public const int LUA_GLOBALSINDEX = (-10002);

    public LuaFunctionsLoader()
    {
        //_logger = IoC.GetInstance<ILogger>();

        //_logger.Information("Log from LuaFunctionsLoader");

        for (int i = 0; i < scriptEnv.Length; i++)
        {
            scriptEnv[i] = new ScriptEnvironment();
        }
    }

    public static string GetErrorDesc(ErrorCodeType code)
    {
        switch (code)
        {
            case ErrorCodeType.LUA_ERROR_PLAYER_NOT_FOUND:
                return "Player not found";
            case ErrorCodeType.LUA_ERROR_CREATURE_NOT_FOUND:
                return "Creature not found";
            case ErrorCodeType.LUA_ERROR_NPC_NOT_FOUND:
                return "Npc not found";
            case ErrorCodeType.LUA_ERROR_NPC_TYPE_NOT_FOUND:
                return "Npc type not found";
            case ErrorCodeType.LUA_ERROR_MONSTER_NOT_FOUND:
                return "Monster not found";
            case ErrorCodeType.LUA_ERROR_MONSTER_TYPE_NOT_FOUND:
                return "Monster type not found";
            case ErrorCodeType.LUA_ERROR_ITEM_NOT_FOUND:
                return "Item not found";
            case ErrorCodeType.LUA_ERROR_THING_NOT_FOUND:
                return "Thing not found";
            case ErrorCodeType.LUA_ERROR_TILE_NOT_FOUND:
                return "Tile not found";
            case ErrorCodeType.LUA_ERROR_HOUSE_NOT_FOUND:
                return "House not found";
            case ErrorCodeType.LUA_ERROR_COMBAT_NOT_FOUND:
                return "Combat not found";
            case ErrorCodeType.LUA_ERROR_CONDITION_NOT_FOUND:
                return "Condition not found";
            case ErrorCodeType.LUA_ERROR_AREA_NOT_FOUND:
                return "Area not found";
            case ErrorCodeType.LUA_ERROR_CONTAINER_NOT_FOUND:
                return "Container not found";
            case ErrorCodeType.LUA_ERROR_VARIANT_NOT_FOUND:
                return "Variant not found";
            case ErrorCodeType.LUA_ERROR_VARIANT_UNKNOWN:
                return "Unknown variant type";
            case ErrorCodeType.LUA_ERROR_SPELL_NOT_FOUND:
                return "Spell not found";
            case ErrorCodeType.LUA_ERROR_ACTION_NOT_FOUND:
                return "Action not found";
            case ErrorCodeType.LUA_ERROR_TALK_ACTION_NOT_FOUND:
                return "TalkAction not found";
            case ErrorCodeType.LUA_ERROR_ZONE_NOT_FOUND:
                return "Zone not found";
            default:
                return "Bad error code";
        }
    }

    public static int ProtectedCall(LuaState luaState, int nargs, int nresults)
    {
        var ret = 0;
        if (ValidateDispatcherContext(nameof(ProtectedCall)))
        {
            return ret;
        }

        int errorIndex = Lua.GetTop(luaState) - nargs;
        //int errorIndex = -1 - nargs - 1;
        Lua.PushCFunction(luaState, LuaErrorHandler);
        Lua.Insert(luaState, errorIndex);

        //int before = Lua.GetTop(luaState);

        ret = Lua.PCall(luaState, nargs, nresults, errorIndex);
        Lua.Remove(luaState, errorIndex);
        return ret;
    }

    public void ReportError(string errorDesc)
    {
        ReportError("__FUNCTION__", errorDesc);
    }

    public static void ReportError(string function, string errorDesc, bool stackTrace = false)
    {
        int scriptId, callbackId;
        bool timerEvent;
        LuaScriptInterface scriptInterface;

        GetScriptEnv().GetEventInfo(out scriptId, out scriptInterface, out callbackId, out timerEvent);

        Console.WriteLine(string.Format("Lua script error: \nscriptInterface: [{0}]\nscriptId: [{1}]\ntimerEvent: [{2}]\n callbackId:[{3}]\nfunction: [{4}]\nerror [{5}]",
                     scriptInterface != null ? scriptInterface.GetInterfaceName() : "",
                     scriptId != 0 ? scriptInterface?.GetFileById(scriptId) : "",
                     timerEvent ? "in a timer event called from:" : "",
                     callbackId != 0 ? scriptInterface?.GetFileById(callbackId) : "",
                     function ?? "",
                     (stackTrace && scriptInterface != null) ? scriptInterface.GetStackTrace(errorDesc) : errorDesc));
    }

    public static int LuaErrorHandler(LuaState luaState)
    {
        string errorMessage = PopString(luaState);
        LuaScriptInterface scriptInterface = GetScriptEnv().GetScriptInterface();
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
            default:
                break;
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

        if (thing == null)
        {
            Lua.CreateTable(luaState, 0, 4);
            SetField(luaState, "uid", 0);
            SetField(luaState, "itemid", 0);
            SetField(luaState, "actionid", 0);
            SetField(luaState, "type", 0);
            return;
        }

        if (thing is IItem item)
        {
            PushUserdata(luaState, item);
            SetItemMetatable(luaState, -1, item);
        }
        else if (thing is ICreature creature)
        {
            PushUserdata(luaState, creature);
            SetCreatureMetatable(luaState, -1, creature);
        }
        else
        {
            Lua.PushNil(luaState);
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

        Lua.RawGetI(luaState, LUA_REGISTRYINDEX, callback);
    }

    public static string PopString(LuaState luaState)
    {
        if (Lua.GetTop(luaState) == 0)
        {
            return string.Empty;
        }

        string str = GetString(luaState, -1);
        Lua.Pop(luaState, 1);
        return str;
    }

    public static int PopCallback(LuaState luaState)
    {
        return Lua.Ref(luaState, LUA_REGISTRYINDEX);
    }

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
        HashSet<string> weakObjectTypes = new HashSet<string>();

        if (ValidateDispatcherContext(nameof(SetWeakMetatable)))
        {
            return;
        }

        string weakName = name + "_weak";

        if (weakObjectTypes.Add(name))
        {
            Lua.GetMetaTable(luaState, name);
            int childMetatable = Lua.GetTop(luaState);

            Lua.NewMetaTable(luaState, weakName);
            int metatable = Lua.GetTop(luaState);

            List<string> methodKeys = new List<string> { "__index", "__metatable", "__eq" };
            foreach (string metaKey in methodKeys)
            {
                Lua.GetField(luaState, childMetatable, metaKey);
                Lua.SetField(luaState, metatable, metaKey);
            }

            List<int> methodIndexes = new List<int> { 'h', 'p', 't' };
            foreach (int metaIndex in methodIndexes)
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

        if (item != null && item.IsContainer)
        {
            Lua.GetMetaTable(luaState, "Container");
        }
        else if (item != null && item.IsTeleport)
        {
            Lua.GetMetaTable(luaState, "Teleport");
        }
        else
        {
            Lua.GetMetaTable(luaState, "Item");
        }

        Lua.SetMetaTable(luaState, index - 1);
    }

    public static void SetCreatureMetatable(LuaState luaState, int index, ICreature creature)
    {
        if (ValidateDispatcherContext(nameof(SetCreatureMetatable)))
        {
            return;
        }

        if (creature != null && creature is IPlayer)
        {
            Lua.GetMetaTable(luaState, "Player");
        }
        else if (creature != null && creature is IMonster)
        {
            Lua.GetMetaTable(luaState, "Monster");
        }
        else
        {
            Lua.GetMetaTable(luaState, "Npc");
        }

        Lua.SetMetaTable(luaState, index - 1);
    }

    public static string GetFormatedLoggerMessage(LuaState luaState)
    {
        string format = GetString(luaState, 1);
        int n = Lua.GetTop(luaState);
        var args = new List<object>();

        for (int i = 2; i <= n; i++)
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
                LuaDataType userType = GetUserdataType(luaState, i);
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
            List<string> indexedArguments = args.Select((arg, index) => $"{{{index}}}").ToList();
            string formattedMessage = string.Format(format, indexedArguments.ToArray());
            return formattedMessage;
            //return fmt.vformat(format, args);
        }
        catch (Exception e)
        {
            Console.WriteLine("[{}] format error: {}", nameof(GetFormatedLoggerMessage), e.Message);
        }

        return string.Empty;
    }

    public static string GetString(LuaState luaState, int arg)
    {
        ulong len = 0;
        var c_str = Lua.ToLString(luaState, arg, ref len);
        if (c_str == null || len == 0)
        {
            return "";
        }
        return c_str;
    }

    public static Location GetPosition(LuaState luaState, int arg, out int stackpos)
    {
        Location position = new Location
        {
            X = GetField<ushort>(luaState, arg, "x"),
            Y = GetField<ushort>(luaState, arg, "y"),
            Z = GetField<byte>(luaState, arg, "z")
        };

        Lua.GetField(luaState, arg, "stackpos");
        if (Lua.IsNil(luaState, -1))
        {
            stackpos = 0;
        }
        else
        {
            stackpos = GetNumber<int>(luaState, -1);
        }

        Lua.Pop(luaState, 4);
        return position;
    }

    public static Location GetPosition(LuaState luaState, int arg)
    {
        Location position = new Location
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
        LuaVariant var = new LuaVariant
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

        LuaDataType type = GetNumber<LuaDataType>(luaState, -1);
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
        int methods = Lua.GetTop(luaState);

        // methodsTable = {}
        Lua.NewTable(luaState);
        int methodsTable = Lua.GetTop(luaState);

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
        int metatable = Lua.GetTop(luaState);

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

    private static int scriptEnvIndex = 0;
    private static ScriptEnvironment[] scriptEnv = new ScriptEnvironment[16];

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
        int parameters = Lua.GetTop(luaState);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return GetNumber<T>(luaState, arg);
    }

    public static T GetUserdataShared<T>(LuaState luaState, int arg) where T : struct
    {
        IntPtr userdata = (IntPtr)Lua.ToUserData(luaState, arg);

        if (userdata == IntPtr.Zero)
        {
            return default(T);
        }

        IntPtr ptr = Marshal.ReadIntPtr(userdata);
        return (T)Marshal.PtrToStructure(ptr, typeof(T));
    }

    public static T GetUserdata<T>(LuaState luaState, int arg) where T : class
    {
        var userdata = GetRawUserdata<T>(luaState, arg);
        if (userdata == IntPtr.Zero)
        {
            return null;
        }

        var stru = (UserDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(System.Runtime.InteropServices.Marshal.ReadIntPtr(userdata), typeof(UserDataStruct));

        lock (_objects)
        {
            if (_objects.TryGetValue(stru.Index, out var value))
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

        var stru = (UserDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(System.Runtime.InteropServices.Marshal.ReadIntPtr(userdata), typeof(UserDataStruct));
        
        lock (_objects)
        {
            if (_objects.TryGetValue(stru.Index, out var value))
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
        int parameters = Lua.GetTop(luaState);
        if (parameters == 0 || arg > parameters)
        {
            return defaultValue;
        }
        return Lua.ToBoolean(luaState, arg);
    }

    public static string GetString(LuaState luaState, int arg, string defaultValue)
    {
        int parameters = Lua.GetTop(luaState);
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
        if (scriptEnvIndex < 0 || scriptEnvIndex >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return scriptEnv[scriptEnvIndex];
    }

    public bool InternalReserveScriptEnv()
    {
        return ++scriptEnvIndex < 16;
    }

    public static ScriptEnvironment GetScriptEnv()
    {
        if (scriptEnvIndex < 0 || scriptEnvIndex >= 16)
        {
            throw new IndexOutOfRangeException();
        }
        return scriptEnv[scriptEnvIndex];
    }

    public static bool ReserveScriptEnv()
    {
        return ++scriptEnvIndex < 16;
    }

    public static void ResetScriptEnv()
    {
        if (scriptEnvIndex < 0)
        {
            throw new IndexOutOfRangeException();
        }
        scriptEnv[scriptEnvIndex--].ResetEnv();
    }

    /// <summary>
    /// Compatibility NewIndexedUserData with constant parameter
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static IntPtr NewUserData(LuaState luaState, int size)
    {
        return (IntPtr)Lua.NewUserData(luaState, (UIntPtr)size);
    }

    public static void NewUData(LuaState luaState, int val)
    {
        IntPtr pointer = NewUserData(luaState, Marshal.SizeOf(typeof(int)));
        Marshal.WriteInt32(pointer, val);
    }

    public static T ToObject<T>(LuaState luaState, int index, bool freeGCHandle = true)
    {
        if (IsNil(luaState, index)/* || !IsLightUserData(index)*/)
            return default(T);

        IntPtr data = (IntPtr)Lua.ToUserData(luaState, index);
        if (data == IntPtr.Zero)
            return default(T);

        var handle = GCHandle.FromIntPtr(data);
        if (!handle.IsAllocated)
            return default(T);

        var reference = (T)handle.Target;

        if (freeGCHandle)
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
    static readonly ConcurrentDictionary<object, int> _objectsBackMap = new ConcurrentDictionary<object, int>(new ReferenceComparer());
    // object # to object (FIXME - it should be possible to get object address as an object #)
    static readonly ConcurrentDictionary<int, object> _objects = new ConcurrentDictionary<int, object>();
    static readonly List<UserDataStruct> _structuresToReuse = new List<UserDataStruct>();

    static readonly ConcurrentQueue<int> finalizedReferences = new ConcurrentQueue<int>();

    //internal EventHandlerContainer PendingEvents = new EventHandlerContainer();

    /// <summary>
    /// We want to ensure that objects always have a unique ID
    /// </summary>
    static int _nextObj;

    private static int AddObject(object obj)
    {
        // New object: inserts it in the list
        int index = _nextObj++;

        lock (_objects)
        {
            _objects[index] = obj;

            if (!obj.GetType().IsValueType || obj.GetType().IsEnum)
                _objectsBackMap[obj] = index;
        }
       
        return index;
    }

    public static void PushUserdata(LuaState luaState, object o)
    {
        int index = -1;

        // Pushes nil
        if (o == null)
        {
            Lua.PushNil(luaState);
            return;
        }

        // Object already in the list of Lua objects? Push the stored reference.
        bool found = (!o.GetType().IsValueType || o.GetType().IsEnum) && _objectsBackMap.TryGetValue(o, out index);

        index = AddObject(o);

        IntPtr userdata;

        UserDataStruct stru;

        if (_structuresToReuse.Any())
        {
            stru = _structuresToReuse.FirstOrDefault();
            _structuresToReuse.Remove(stru);

            stru.Index = index;
            userdata = stru.Ptr;
        }
        else
        {
            userdata = (IntPtr)Lua.NewUserData(luaState, (ulong)IntPtr.Size);
            stru = new UserDataStruct(index, userdata, 0);
        }

        GCHandle gch2 = GCHandle.Alloc(stru, GCHandleType.Pinned);

        Marshal.WriteIntPtr(userdata, gch2.AddrOfPinnedObject());
        gch2.Free();
    }

    public static void UpdateLuaUserdata(LuaState luaState, int index, IItem newItem)
    {
        var userdata = Lua.ToUserData(luaState, index);
        if (userdata == IntPtr.Zero)
            throw new Exception("Failed to update userdata: userdata not found.");

        var userDataStruct = (UserDataStruct)Marshal.PtrToStructure(
            Marshal.ReadIntPtr(userdata),
            typeof(UserDataStruct)
        );

        lock (_objects)
            if (_objects.ContainsKey(userDataStruct.Index))
                _objects[userDataStruct.Index] = newItem;
    }
}
