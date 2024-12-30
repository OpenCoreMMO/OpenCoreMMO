using LuaNET;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT;

public class LuaScriptInterface : LuaFunctionsLoader, ILuaScriptInterface
{
    public static int EVENT_ID_LOADING = 1;
    public static int EVENT_ID_USER = 1000;
    private readonly string interfaceName;

    protected Dictionary<int, string> cacheFiles;

    protected int eventTableRef;

    private string lastLuaError;
    private string loadedScriptName;
    private string loadingFile;
    protected LuaState luaState;
    protected int runningEventId = EVENT_ID_USER;

    //private ILuaEnvironment _luaEnvironment = null;

    //private ILuaEnvironment g_luaEnvironment()
    //{
    //    if(_luaEnvironment == null)
    //        _luaEnvironment = IoC.GetInstance<ILuaEnvironment>();

    //    return _luaEnvironment;
    //}

    public LuaScriptInterface(string initInterfaceName)
    {
        interfaceName = initInterfaceName;
    }

    public bool ReInitState()
    {
        //g_luaEnvironment().ClearCombatObjects(this);
        //g_luaEnvironment().ClearAreaObjects(this);

        CloseState();
        return InitState();
    }

    public bool LoadFile(string file, string scriptName)
    {
        var ret = Lua.LoadFile(luaState, file);
        if (ret != 0)
        {
            lastLuaError = PopString(luaState);
            return false;
        }

        if (!IsFunction(luaState, -1)) return false;

        loadingFile = file;

        SetLoadingScriptName(scriptName);

        if (!ReserveScriptEnv()) return false;

        var env = GetScriptEnv();
        env.SetScriptId(EVENT_ID_LOADING, this);

        ret = ProtectedCall(luaState, 0, 0);
        if (ret != 0)
        {
            ReportError(null, PopString(luaState));
            ResetScriptEnv();
            return false;
        }

        ResetScriptEnv();
        return true;
    }

    public int GetEvent(string eventName)
    {
        Lua.RawGetI(luaState, LUA_REGISTRYINDEX, eventTableRef);
        if (!IsTable(luaState, -1))
        {
            Lua.Pop(luaState, 1);
            return -1;
        }

        Lua.GetGlobal(luaState, eventName);
        if (!IsFunction(luaState, -1))
        {
            Lua.Pop(luaState, 2);
            return -1;
        }

        Lua.PushValue(luaState, -1);
        Lua.RawSetI(luaState, -3, runningEventId);
        Lua.Pop(luaState, 2);

        Lua.PushNil(luaState);
        Lua.SetGlobal(luaState, eventName);

        cacheFiles[runningEventId] = loadingFile + ":" + eventName;
        return runningEventId++;
    }

    public int GetEvent()
    {
        if (!IsFunction(luaState, -1)) return -1;

        Lua.RawGetI(luaState, LUA_REGISTRYINDEX, eventTableRef);
        if (!IsTable(luaState, -1))
        {
            Lua.Pop(luaState, 1);
            return -1;
        }

        Lua.PushValue(luaState, -2);
        Lua.RawSetI(luaState, -2, runningEventId);
        Lua.Pop(luaState, 2);

        cacheFiles[runningEventId] = loadingFile + ":callback";
        return runningEventId++;
    }

    public int GetMetaEvent(string globalName, string eventName)
    {
        Lua.RawGetI(luaState, LUA_REGISTRYINDEX, eventTableRef);
        if (!IsTable(luaState, -1))
        {
            Lua.Pop(luaState, 1);
            return -1;
        }

        Lua.GetGlobal(luaState, globalName);
        Lua.GetField(luaState, -1, eventName);
        if (!IsFunction(luaState, -1))
        {
            Lua.Pop(luaState, 3);
            return -1;
        }

        Lua.PushValue(luaState, -1);
        Lua.RawSetI(luaState, -4, runningEventId);
        Lua.Pop(luaState, 1);

        Lua.PushNil(luaState);
        Lua.SetField(luaState, -2, eventName);
        Lua.Pop(luaState, 2);

        cacheFiles[runningEventId] = loadingFile + ":" + globalName + "@" + eventName;
        return runningEventId++;
    }

    public string GetFileById(int scriptId)
    {
        if (scriptId == EVENT_ID_LOADING) return loadingFile;

        if (cacheFiles.TryGetValue(scriptId, out var file)) return file;

        return "(Unknown scriptfile)";
    }

    public string GetStackTrace(string errorDesc)
    {
        Lua.GetGlobal(luaState, "debug");
        if (!IsTable(luaState, -1))
        {
            Lua.Pop(luaState, 1);
            return errorDesc;
        }

        Lua.GetField(luaState, -1, "traceback");
        if (!IsFunction(luaState, -1))
        {
            Lua.Pop(luaState, 2);
            return errorDesc;
        }

        Lua.Replace(luaState, -2);
        PushString(luaState, errorDesc);
        Lua.Call(luaState, 1, 1);
        return PopString(luaState);
    }

    public bool PushFunction(int functionId)
    {
        Lua.RawGetI(luaState, LUA_REGISTRYINDEX, eventTableRef);
        if (!IsTable(luaState, -1)) return false;

        Lua.RawGetI(luaState, -1, functionId);
        Lua.Replace(luaState, -2);
        return IsFunction(luaState, -1);
    }

    public bool InitState()
    {
        luaState = g_luaEnvironment().GetLuaState();
        if (luaState.IsNull) return false;

        Lua.OpenLibs(luaState);

        Lua.NewTable(luaState);
        eventTableRef = Lua.Ref(luaState, LUA_REGISTRYINDEX);
        runningEventId = EVENT_ID_USER;

        cacheFiles = new Dictionary<int, string>();

        return true;
    }

    public bool CloseState()
    {
        if (g_luaEnvironment().IsShuttingDown()) luaState.pointer = 0;

        if (luaState.IsNull || g_luaEnvironment().GetLuaState().pointer == 0) return false;

        cacheFiles.Clear();
        if (eventTableRef != -1)
        {
            Lua.UnRef(luaState, LUA_REGISTRYINDEX, eventTableRef);
            eventTableRef = -1;
        }

        luaState.pointer = 0;
        return true;
    }

    public bool CallFunction(int parameters)
    {
        var result = false;
        var size = Lua.GetTop(luaState);
        if (ProtectedCall(luaState, parameters, 1) != 0)
            ReportError(null, GetString(luaState, -1));
        else
            result = GetBoolean(luaState, -1);

        Lua.Pop(luaState, 1);
        if (Lua.GetTop(luaState) + parameters + 1 != size) ReportError(null, "Stack size changed!");

        ResetScriptEnv();
        return result;
    }

    public void CallVoidFunction(int parameters)
    {
        var size = Lua.GetTop(luaState);
        if (ProtectedCall(luaState, parameters, 0) != 0) ReportError(null, PopString(luaState));

        if (Lua.GetTop(luaState) + parameters + 1 != size) ReportError(null, "Stack size changed!");

        ResetScriptEnv();
    }

    public string GetInterfaceName()
    {
        return interfaceName;
    }

    public string GetLastLuaError()
    {
        return lastLuaError;
    }

    public string GetLoadingFile()
    {
        return loadingFile;
    }

    public string GetLoadingScriptName()
    {
        // If scripty name is empty, return warning informing
        if (string.IsNullOrEmpty(loadedScriptName))
            //g_logger().warn("[LuaScriptInterface::getLoadingScriptName] - Script name is empty");
            Console.WriteLine("[LuaScriptInterface::getLoadingScriptName] - Script name is empty");

        return loadedScriptName;
    }

    public void SetLoadingScriptName(string scriptName)
    {
        loadedScriptName = scriptName;
    }

    private LuaEnvironment g_luaEnvironment()
    {
        return LuaEnvironment.GetInstance();
    }

    ~LuaScriptInterface()
    {
        CloseState();
    }

    public virtual LuaState GetLuaState()
    {
        return luaState;
    }
}