using LuaNET;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaEnvironment : LuaScriptInterface, ILuaEnvironment
{
    #region Members

    private static LuaEnvironment _instance = null;

    private static bool shuttingDown = false;

    //private static readonly Dictionary<LuaScriptInterface, List<uint>> combatIdMap = new Dictionary<LuaScriptInterface, List<uint>>();
    //private static readonly Dictionary<uint, Combat> combatMap = new Dictionary<uint, Combat>();
    //private static uint lastCombatId = 0;

    //private static readonly Dictionary<LuaScriptInterface, List<uint>> areaIdMap = new Dictionary<LuaScriptInterface, List<uint>>();
    //private static readonly Dictionary<uint, AreaCombat> areaMap = new Dictionary<uint, AreaCombat>();
    //private static uint lastAreaId = 0;

    //private static uint lastWeaponId;
    //private static Dictionary<uint, YourWeaponType> weaponMap = new Dictionary<uint, YourWeaponType>();
    //private static Dictionary<LuaScriptInterface, List<uint>> weaponIdMap = new Dictionary<LuaScriptInterface, List<uint>>();

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

    //public Combat GetCombatObject(uint id)
    //{
    //    if (combatMap.TryGetValue(id, out var combat))
    //    {
    //        return combat;
    //    }
    //    return null;
    //}

    //public Combat CreateCombatObject(LuaScriptInterface @interface)
    //{
    //    var combat = new Combat();
    //    combatMap[++lastCombatId] = combat;
    //    if (!combatIdMap.ContainsKey(@interface))
    //    {
    //        combatIdMap[@interface] = new List<uint>();
    //    }
    //    combatIdMap[@interface].Add(lastCombatId);
    //    return combat;
    //}

    //public void ClearCombatObjects(LuaScriptInterface @interface)
    //{
    //    if (combatIdMap.TryGetValue(@interface, out var combatIds))
    //    {
    //        foreach (var id in combatIds)
    //        {
    //            if (combatMap.TryGetValue(id, out var combat))
    //            {
    //                combatMap.Remove(id);
    //            }
    //        }
    //        combatIds.Clear();
    //    }
    //}

    //public AreaCombat GetAreaObject(uint id)
    //{
    //    if (areaMap.TryGetValue(id, out var areaCombat))
    //    {
    //        return areaCombat;
    //    }
    //    return null;
    //}

    //public uint CreateAreaObject(LuaScriptInterface @interface)
    //{
    //    areaMap[++lastAreaId] = new AreaCombat();
    //    if (!areaIdMap.ContainsKey(@interface))
    //    {
    //        areaIdMap[@interface] = new List<uint>();
    //    }
    //    areaIdMap[@interface].Add(lastAreaId);
    //    return lastAreaId;
    //}

    //public void ClearAreaObjects(LuaScriptInterface @interface)
    //{
    //    if (areaIdMap.TryGetValue(@interface, out var areaIds))
    //    {
    //        foreach (var id in areaIds)
    //        {
    //            if (areaMap.TryGetValue(id, out var areaCombat))
    //            {
    //                areaMap.Remove(id);
    //            }
    //        }
    //        areaIds.Clear();
    //    }
    //}

    //public static T CreateWeaponObject<T>(LuaScriptInterface interfaceInstance) where T : YourWeaponType, new()
    //{
    //    var weapon = new T(interfaceInstance);
    //    var weaponId = ++lastWeaponId;
    //    weaponMap[weaponId] = weapon;
    //    if (!weaponIdMap.ContainsKey(interfaceInstance))
    //    {
    //        weaponIdMap[interfaceInstance] = new List<uint>();
    //    }
    //    weaponIdMap[interfaceInstance].Add(weaponId);
    //    return weapon;
    //}

    //public static T GetWeaponObject<T>(uint id) where T : YourWeaponType
    //{
    //    if (weaponMap.TryGetValue(id, out var weapon))
    //    {
    //        return weapon;
    //    }
    //    return null;
    //}

    //public static void ClearWeaponObjects(LuaScriptInterface interfaceInstance)
    //{
    //    if (weaponIdMap.TryGetValue(interfaceInstance, out var weaponIds))
    //    {
    //        weaponIds.Clear();
    //    }
    //    weaponMap.Clear();
    //}

    public static bool IsShuttingDown()
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
