using LuaNET;
using Microsoft.Extensions.Configuration;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ConfigFunctions : LuaScriptInterface, IConfigFunctions
{
    private static IConfigManager _configManager;
    private static ConfigurationMap _configurationMap;
    private static ServerConfiguration _serverConfiguration;

    public ConfigFunctions(
        IConfigManager configManager,
        ConfigurationMap configurationMap,
        ServerConfiguration serverConfiguration) : base(nameof(ConfigFunctions))
    {
        _configManager = configManager;
        _configurationMap = configurationMap;
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
         RegisterEnumIn<BooleanConfigType>(luaState, "configKeys");
         RegisterEnumIn<StringConfigType>(luaState, "configKeys");
         RegisterEnumIn<IntegerConfigType>(luaState, "configKeys");
         RegisterEnumIn<FloatingConfigType>(luaState, "configKeys");
        

        // foreach (var item in Enum.GetValues<BooleanConfigType>())
        //     RegisterVariable(luaState, "configKeys", item.ToString(), item);
        //
        // foreach (var item in Enum.GetValues<StringConfigType>())
        //     RegisterVariable(luaState, "configKeys", item.ToString(), item);
        //
        // foreach (var item in Enum.GetValues<IntegerConfigType>())
        //     RegisterVariable(luaState, "configKeys", item.ToString(), item);
        //
        // foreach (var item in Enum.GetValues<FloatingConfigType>())
        //     RegisterVariable(luaState, "configKeys", item.ToString(), item);

        RegisterVariable(luaState, "configKeys", "BASE_DIRECTORY", _serverConfiguration.Data);
    }

    public static int LuaConfigManagerGetFloat(LuaState luaState)
    {
        // configManager:getFloat()
        Lua.PushNumber(luaState, _configManager.GetFloat(GetNumber<FloatingConfigType>(luaState, -1)));
        return 1;
    }

    public static int LuaConfigManagerGetString(LuaState luaState)
    {
        // configManager:getString()
        PushString(luaState, _configManager.GetString(GetNumber<StringConfigType>(luaState, -1)));
        return 1;
    }

    public static int LuaConfigManagerGetNumber(LuaState luaState)
    {
        // configManager:getNumber()
        Lua.PushNumber(luaState, _configManager.GetNumber(GetNumber<IntegerConfigType>(luaState, -1)));
        return 1;
    }

    public static int LuaConfigManagerGetBoolean(LuaState luaState)
    {
        // configManager:getBoolean()
        var config = GetNumber<BooleanConfigType>(luaState, -1);

        var configValue = _configurationMap.GetBoolean(config);
        
        PushBoolean(luaState, configValue);
        return 1;
    }
}