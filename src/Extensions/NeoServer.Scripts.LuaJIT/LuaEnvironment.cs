using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaEnvironment : LuaScriptInterface, ILuaEnvironment
{
    #region Injection

    /// <summary>
    ///     A reference to the logger in use.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Members

    private static LuaEnvironment _instance;

    private static bool _shuttingDown;

    public Dictionary<uint, LuaTimerEventDesc> TimerEvents { get; } = new();
    public uint LastEventTimerId { get; set; } = 1;

    private static readonly List<string> CacheFiles = [];

    private static LuaScriptInterface _testInterface;

    #endregion

    #region Instance

    public static LuaEnvironment GetInstance()
    {
        return _instance == null ? _instance = new LuaEnvironment() : _instance;
    }

    public LuaEnvironment() : base("Main Interface")
    {
    }

    #endregion

    #region Constructors

    public LuaEnvironment(ILogger logger) : base("Main Interface")
    {
        _instance = this;

        _logger = logger.ForContext<LuaEnvironment>();
    }

    ~LuaEnvironment()
    {
        if (_testInterface == null)
        {
        }

        _shuttingDown = true;
        CloseState();
    }

    #endregion

    #region Public Methods

    public override LuaState GetLuaState()
    {
        if (_shuttingDown) return luaState;

        if (luaState.IsNull) InitState();

        return luaState;
    }

    public new bool InitState()
    {
        luaState = Lua.NewState();
        //LuaFunctionsLoader.Load(luaState);
        runningEventId = EVENT_ID_USER;

        return true;
    }

    public new bool ReInitState()
    {
        // TODO: get children, reload children
        CloseState();
        return InitState();
    }

    public new bool CloseState()
    {
        if (luaState.IsNull) return false;

        //foreach (var combatEntry in combatIdMap)
        //{
        //    ClearCombatObjects(combatEntry.Key);
        //}

        //foreach (var areaEntry in areaIdMap)
        //{
        //    ClearAreaObjects(areaEntry.Key);
        //}

        foreach (var timerEntry in TimerEvents)
        {
            var timerEventDesc = timerEntry.Value;
            foreach (var parameter in timerEventDesc.Parameters) Lua.UnRef(luaState, LUA_REGISTRY_INDEX, parameter);
            Lua.UnRef(luaState, LUA_REGISTRY_INDEX, timerEventDesc.Function);
        }

        //combatIdMap.Clear();
        //areaIdMap.Clear();
        TimerEvents.Clear();
        CacheFiles.Clear();

        Lua.Close(luaState);
        luaState.pointer = 0;
        return true;
    }

    public LuaScriptInterface GetTestInterface()
    {
        if (_testInterface != null) return _testInterface;
        
        _testInterface = new LuaScriptInterface("Test Interface");
        _testInterface.InitState();

        return _testInterface;
    }

    public bool IsShuttingDown()
    {
        return _shuttingDown;
    }

    public void ExecuteTimerEvent(uint eventIndex)
    {
        if (!TimerEvents.Remove(eventIndex, out var timerEventDesc)) return;
        
        Lua.RawGetI(luaState, LUA_REGISTRY_INDEX, timerEventDesc.Function);

        var reverseList = timerEventDesc.Parameters.ToList();
        reverseList.Reverse();

        foreach (var parameter in reverseList) Lua.RawGetI(luaState, LUA_REGISTRY_INDEX, parameter);

        if (ReserveScriptEnv())
        {
            var env = GetScriptEnv();
            env.SetTimerEvent();
            env.SetScriptId(timerEventDesc.ScriptId, this);
            CallFunction(timerEventDesc.Parameters.Count);
        }
        else
        {
            _logger.Error(
                "[LuaEnvironment::executeTimerEvent - Lua file {LoadingFile}] Call stack overflow. Too many lua script calls being nested",
                GetLoadingFile());
        }

        Lua.UnRef(luaState, LUA_REGISTRY_INDEX, timerEventDesc.Function);
        foreach (var parameter in timerEventDesc.Parameters) Lua.UnRef(luaState, LUA_REGISTRY_INDEX, parameter);
    }

    public void CollectGarbage()
    {
        for (var i = -1; ++i < 2;) Lua.GC(luaState, LuaGCParam.Collect, 0);
    }

    #endregion
}