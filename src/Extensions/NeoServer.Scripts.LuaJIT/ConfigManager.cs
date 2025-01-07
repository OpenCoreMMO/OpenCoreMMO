using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class ConfigManager : IConfigManager
{
    private static readonly string DummyStr = string.Empty;

    #region Injection

    /// <summary>
    ///     A reference to the logger in use.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    private string configFileLua = "";

    public ConfigManager(ILogger logger)
    {
        _logger = logger.ForContext<ConfigManager>();
    }

    public bool Load(string file)
    {
        configFileLua = file.Split("\\").LastOrDefault();
        var luaState = Lua.NewState();
        if (luaState.pointer == 0) throw new IOException("Failed to allocate memory");

        Lua.OpenLibs(luaState);

        if (Lua.DoFile(luaState, file) > 0)
        {
            _logger.Error("[ConfigManager::load] - {0}", Lua.ToString(luaState, -1));
            Lua.Close(luaState);
            return false;
        }

        // Parse config
        // Info that must be loaded one time (unless we reset the modules involved)
        if (!loaded)
        {
            integerConfig[(int)IntegerConfigType.GAME_PORT] = GetGlobalNumber(luaState, "gameProtocolPort", 7172);
            integerConfig[(int)IntegerConfigType.LOGIN_PORT] = GetGlobalNumber(luaState, "loginProtocolPort", 7171);
            integerConfig[(int)IntegerConfigType.STATUS_PORT] = GetGlobalNumber(luaState, "statusProtocolPort", 7171);
        }

        booleanConfig[(int)BooleanConfigType.SCRIPTS_CONSOLE_LOGS] =
            GetGlobalBoolean(luaState, "showScriptsLogInConsole", true);

        booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL] = GetGlobalBoolean(luaState, "toggleSaveInterval", false);
        booleanConfig[(int)BooleanConfigType.TOGGLE_SAVE_INTERVAL_CLEAN_MAP] =
            GetGlobalBoolean(luaState, "toggleSaveIntervalCleanMap", false);
        booleanConfig[(int)BooleanConfigType.ALLOW_RELOAD] = GetGlobalBoolean(luaState, "allowReload", true);

        stringConfig[(int)StringConfigType.SAVE_INTERVAL_TYPE] = GetGlobalString(luaState, "saveIntervalType", "");

        stringConfig[(int)StringConfigType.CORE_DIRECTORY] = GetGlobalString(luaState, "coreDirectory", "data");

        integerConfig[(int)IntegerConfigType.SAVE_INTERVAL_TIME] = GetGlobalNumber(luaState, "saveIntervalTime", 1);

        loaded = true;
        Lua.Close(luaState);
        return true;
    }

    public string GetString(StringConfigType what)
    {
        if (what >= StringConfigType.LAST_STRING_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetString] - Accessing invalid index: {what}");
            return string.Empty;
        }

        return stringConfig[(int)what];
    }

    public int GetNumber(IntegerConfigType what)
    {
        if (what >= IntegerConfigType.LAST_INTEGER_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetNumber] - Accessing invalid index: {what}");
            return 0;
        }

        return integerConfig[(int)what];
    }

    public short GetShortNumber(IntegerConfigType what)
    {
        if (what >= IntegerConfigType.LAST_INTEGER_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetShortNumber] - Accessing invalid index: {what}");
            return 0;
        }

        return (short)integerConfig[(int)what];
    }

    public ushort GetUShortNumber(IntegerConfigType what)
    {
        return (ushort)integerConfig[(int)what];
    }

    public bool GetBoolean(BooleanConfigType what)
    {
        if (what >= BooleanConfigType.LAST_BOOLEAN_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetBoolean] - Accessing invalid index: {what}");
            return false;
        }

        return booleanConfig[(int)what];
    }

    public float GetFloat(FloatingConfigType what)
    {
        if (what >= FloatingConfigType.LAST_FLOATING_CONFIG)
        {
            _logger.Warning($"[ConfigManager.GetFloat] - Accessing invalid index: {what}");
            return 0;
        }

        return floatingConfig[(int)what];
    }

    public string SetConfigFileLua(string what)
    {
        return configFileLua = what;
    }

    public string GetConfigFileLua()
    {
        return configFileLua;
    }

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

    private readonly string[] stringConfig = new string[(int)StringConfigType.LAST_STRING_CONFIG];
    private readonly int[] integerConfig = new int[(int)IntegerConfigType.LAST_INTEGER_CONFIG];
    private readonly bool[] booleanConfig = new bool[(int)BooleanConfigType.LAST_BOOLEAN_CONFIG];
    private readonly float[] floatingConfig = new float[(int)FloatingConfigType.LAST_FLOATING_CONFIG];

    private bool loaded;

    #endregion
}