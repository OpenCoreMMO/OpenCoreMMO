using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaEnvironment : LuaScriptInterface, ILuaEnvironment
{
    #region Members

    private static LuaEnvironment _instance = null;

    private static bool shuttingDown = false;

    public readonly Dictionary<uint, LuaTimerEventDesc> timerEvents = new Dictionary<uint, LuaTimerEventDesc>();
    public uint LastEventTimerId = 1;

    private static readonly List<string> cacheFiles = new List<string>();

    private static LuaScriptInterface testInterface;

    private static int runningEventId = EVENT_ID_USER;

    #endregion

    #region Injection

    /// <summary>
    /// A reference to the logger in use.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Instance

    public static LuaEnvironment GetInstance() => _instance == null ? _instance = new LuaEnvironment() : _instance;

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
        if (testInterface == null)
        {
        }

        shuttingDown = true;
        CloseState();
    }

    #endregion

    #region Public Methods

    public LuaState GetLuaState()
    {
        if (shuttingDown)
        {
            return luaState;
        }

        if (luaState.IsNull)
        {
            InitState();
        }

        return luaState;
    }

    public bool InitState()
    {
        luaState = Lua.NewState();
        //LuaFunctionsLoader.Load(luaState);
        runningEventId = EVENT_ID_USER;

        return true;
    }

    public bool ReInitState()
    {
        // TODO: get children, reload children
        CloseState();
        return InitState();
    }

    public bool CloseState()
    {
        if (luaState.IsNull)
        {
            return false;
        }

        //foreach (var combatEntry in combatIdMap)
        //{
        //    ClearCombatObjects(combatEntry.Key);
        //}

        //foreach (var areaEntry in areaIdMap)
        //{
        //    ClearAreaObjects(areaEntry.Key);
        //}

        foreach (var timerEntry in timerEvents)
        {
            var timerEventDesc = timerEntry.Value;
            foreach (var parameter in timerEventDesc.Parameters)
            {
                Lua.UnRef(luaState, LUA_REGISTRYINDEX, parameter);
            }
            Lua.UnRef(luaState, LUA_REGISTRYINDEX, timerEventDesc.Function);
        }

        //combatIdMap.Clear();
        //areaIdMap.Clear();
        timerEvents.Clear();
        cacheFiles.Clear();

        Lua.Close(luaState);
        luaState.pointer = 0;
        return true;
    }

    public LuaScriptInterface GetTestInterface()
    {
        if (testInterface == null)
        {
            testInterface = new LuaScriptInterface("Test Interface");
            testInterface.InitState();
        }
        return testInterface;
    }

    public bool IsShuttingDown()
    {
        return shuttingDown;
    }

    public void ExecuteTimerEvent(uint eventIndex)
    {
        if (timerEvents.TryGetValue(eventIndex, out var timerEventDesc))
        {
            timerEvents.Remove(eventIndex);

            Lua.RawGetI(luaState, LUA_REGISTRYINDEX, timerEventDesc.Function);

            var reverseList = timerEventDesc.Parameters.ToList();
            reverseList.Reverse();

            foreach (var parameter in reverseList)
            {
                Lua.RawGetI(luaState, LUA_REGISTRYINDEX, parameter);
            }

            if (ReserveScriptEnv())
            {
                var env = GetScriptEnv();
                env.SetTimerEvent();
                env.SetScriptId(timerEventDesc.ScriptId, this);
                CallFunction(timerEventDesc.Parameters.Count);
            }
            else
            {
                _logger.Error($"[LuaEnvironment::executeTimerEvent - Lua file {GetLoadingFile()}] Call stack overflow. Too many lua script calls being nested");
            }

            Lua.UnRef(luaState, LUA_REGISTRYINDEX, timerEventDesc.Function);
            foreach (var parameter in timerEventDesc.Parameters)
            {
                Lua.UnRef(luaState, LUA_REGISTRYINDEX, parameter);
            }
        }
    }

    public void CollectGarbage()
    {
        bool collecting = false;

        if (!collecting)
        {
            collecting = true;

            for (int i = -1; ++i < 2;)
            {
                Lua.GC(luaState, LuaGCParam.Collect, 0);
            }

            collecting = false;
        }
    }

    #endregion
}
