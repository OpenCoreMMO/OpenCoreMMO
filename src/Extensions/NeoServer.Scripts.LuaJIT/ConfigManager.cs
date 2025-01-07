using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class ConfigManager : IConfigManager
{
    #region Injection

    /// <summary>
    ///     A reference to the logger in use.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    private string _configFileLua = "";

    public ConfigManager(ILogger logger)
    {
        _logger = logger.ForContext<ConfigManager>();
    }

    public bool Load(string file)
    {
        _configFileLua = file.Split("\\").LastOrDefault();
        var luaState = Lua.NewState();
        if (luaState.pointer == 0) throw new IOException("Failed to allocate memory");

        Lua.OpenLibs(luaState);

        if (Lua.DoFile(luaState, file) > 0)
        {
            _logger.Error("[ConfigManager::load] - {LuaError}", Lua.ToString(luaState, -1));
            Lua.Close(luaState);
            return false;
        }

        // Parse config
        // Info that must be loaded one time (unless we reset the modules involved)
        if (!_loaded)
        {
            _integerConfig[(int)IntegerConfigType.GAME_PORT] = GetGlobalNumber(luaState, "gameProtocolPort", 7172);
            _integerConfig[(int)IntegerConfigType.LOGIN_PORT] = GetGlobalNumber(luaState, "loginProtocolPort", 7171);
            _integerConfig[(int)IntegerConfigType.STATUS_PORT] = GetGlobalNumber(luaState, "statusProtocolPort", 7171);
        }

        _booleanConfig[(int)BooleanConfigType.SCRIPTS_CONSOLE_LOGS] =
            GetGlobalBoolean(luaState, "showScriptsLogInConsole", true);

        _booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL] = GetGlobalBoolean(luaState, "toggleSaveInterval", false);
        _booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL_CLEAN_MAP] =
            GetGlobalBoolean(luaState, "toggleSaveIntervalCleanMap", false);
        _booleanConfig[(int)BooleanConfigType.ALLOW_RELOAD] = GetGlobalBoolean(luaState, "allowReload", true);

        _stringConfig[(int)StringConfigType.SAVE_INTERVAL_TYPE] = GetGlobalString(luaState, "saveIntervalType", "");

        _stringConfig[(int)StringConfigType.CORE_DIRECTORY] = GetGlobalString(luaState, "coreDirectory", "data");

        _integerConfig[(int)IntegerConfigType.SAVE_INTERVAL_TIME] = GetGlobalNumber(luaState, "saveIntervalTime", 1);

        _loaded = true;
        Lua.Close(luaState);
        return true;
    }

    public string GetString(StringConfigType what)
    {
        if (what < StringConfigType.LAST_STRING_CONFIG) return _stringConfig[(int)what];
        
        _logger.Warning("[ConfigManager.GetString] - Accessing invalid index: {What}", what);
        return string.Empty;

    }

    public int GetNumber(IntegerConfigType what)
    {
        if (what < IntegerConfigType.LAST_INTEGER_CONFIG) return _integerConfig[(int)what];
        
        _logger.Warning("[ConfigManager.GetNumber] - Accessing invalid index: {What}", what);
        return 0;

    }

    public short GetShortNumber(IntegerConfigType what)
    {
        if (what < IntegerConfigType.LAST_INTEGER_CONFIG) return (short)_integerConfig[(int)what];
        
        _logger.Warning("[ConfigManager.GetShortNumber] - Accessing invalid index: {What}", what);
        return 0;

    }

    public ushort GetUShortNumber(IntegerConfigType what) => (ushort)_integerConfig[(int)what];

    public bool GetBoolean(BooleanConfigType what)
    {
        if (what < BooleanConfigType.LAST_BOOLEAN_CONFIG) return _booleanConfig[(int)what];
        _logger.Warning("[ConfigManager.GetBoolean] - Accessing invalid index: {What}", what);
        return false;
    }

    public float GetFloat(FloatingConfigType what)
    {
        if (what < FloatingConfigType.LAST_FLOATING_CONFIG) return _floatingConfig[(int)what];
        
        _logger.Warning("[ConfigManager.GetFloat] - Accessing invalid index: {What}", what);
        return 0;
    }

    public string SetConfigFileLua(string what) => _configFileLua = what;

    public string GetConfigFileLua() => _configFileLua;

    public string GetGlobalString(LuaState luaState, string identifier, string defaultValue)
    {
        Lua.GetGlobal(luaState, identifier);
        if (Lua.IsString(luaState, -1)) return defaultValue;

        ulong len = 0;
        var str = Lua.ToLString(luaState, -1, ref len);
        Lua.Pop(luaState, 1);
        return str;
    }

    public int GetGlobalNumber(LuaState luaState, string identifier, int defaultValue = 0)
    {
        Lua.GetGlobal(luaState, identifier);
        if (Lua.IsNumber(luaState, -1)) return defaultValue;

        var val = (int)Lua.ToNumber(luaState, -1);
        Lua.Pop(luaState, 1);
        return val;
    }

    public bool GetGlobalBoolean(LuaState luaState, string identifier, bool defaultValue)
    {
        Lua.GetGlobal(luaState, identifier);
        if (Lua.IsBoolean(luaState, -1))
        {
            if (Lua.IsString(luaState, -1)) return defaultValue;

            ulong len = 0;
            var str = Lua.ToLString(luaState, -1, ref len);
            Lua.Pop(luaState, 1);
            return BooleanString(str);
        }

        var val = Lua.ToBoolean(luaState, -1);
        Lua.Pop(luaState, 1);
        return val;
    }

    public float GetGlobalFloat(LuaState luaState, string identifier, float defaultValue = 0.0f)
    {
        Lua.GetGlobal(luaState, identifier);
        if (Lua.IsNumber(luaState, -1)) return defaultValue;

        var val = (float)Lua.ToNumber(luaState, -1);
        Lua.Pop(luaState, 1);
        return val;
    }

    private bool BooleanString(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;

        var ch = str.ToLower();
        return ch != "f" && ch != "n" && ch != "0";
    }

    #region Members

    private readonly string[] _stringConfig = new string[(int)StringConfigType.LAST_STRING_CONFIG];
    private readonly int[] _integerConfig = new int[(int)IntegerConfigType.LAST_INTEGER_CONFIG];
    private readonly bool[] _booleanConfig = new bool[(int)BooleanConfigType.LAST_BOOLEAN_CONFIG];
    private readonly float[] _floatingConfig = new float[(int)FloatingConfigType.LAST_FLOATING_CONFIG];

    private bool _loaded;

    #endregion
}