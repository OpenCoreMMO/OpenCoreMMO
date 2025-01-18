using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ConfigFunctions : LuaScriptInterface, IConfigFunctions
{
    private static IConfigManager _configManager;
    private readonly ServerConfiguration _serverConfiguration;

    public ConfigFunctions(
        IConfigManager configManager,
        ServerConfiguration serverConfiguration) : base(nameof(ConfigFunctions))
    {
        _configManager = configManager;
        _serverConfiguration = serverConfiguration;
    }

    public void Init(LuaState luaState)
    {
        RegisterTable(luaState, "configManager");
        RegisterMethod(luaState, "configManager", "getString", LuaConfigManagerGetString);
        RegisterMethod(luaState, "configManager", "getNumber", LuaConfigManagerGetNumber);
        RegisterMethod(luaState, "configManager", "getBoolean", LuaConfigManagerGetBoolean);
        RegisterMethod(luaState, "configManager", "getFloat", LuaConfigManagerGetFloat);

        RegisterTable(luaState, "configKeys");

        //RegisterVariable(luaState, "configKeys", "ALLOW_CHANGEOUTFIT", BooleanConfig.ALLOW_CHANGEOUTFIT);

        foreach (var item in Enum.GetValues<BooleanConfigType>())
            RegisterVariable(luaState, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<StringConfigType>())
            RegisterVariable(luaState, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<IntegerConfigType>())
            RegisterVariable(luaState, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<FloatingConfigType>())
            RegisterVariable(luaState, "configKeys", item.ToString(), item);

        RegisterVariable(luaState, "configKeys", "BASE_DIRECTORY", AppContext.BaseDirectory + _serverConfiguration.DataLuaJit);
    }

    private static int LuaConfigManagerGetString(LuaState luaState)
    {
        // configManager:getString()
        PushString(luaState, _configManager.GetString(GetNumber<StringConfigType>(luaState, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetNumber(LuaState luaState)
    {
        // configManager:getNumber()
        Lua.PushNumber(luaState, _configManager.GetNumber(GetNumber<IntegerConfigType>(luaState, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetBoolean(LuaState luaState)
    {
        // configManager:getBoolean()
        PushBoolean(luaState, _configManager.GetBoolean(GetNumber<BooleanConfigType>(luaState, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetFloat(LuaState luaState)
    {
        // configManager:getFloat()
        Lua.PushNumber(luaState, _configManager.GetFloat(GetNumber<FloatingConfigType>(luaState, -1)));
        return 1;
    }
}