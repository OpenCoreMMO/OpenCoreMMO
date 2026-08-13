using LuaNET;
using NeoServer.Domain.Common;
using NeoServer.Scripts.LuaJIT.Enums.Config;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.Functions;

public class ConfigFunctions : LuaScriptInterface, IConfigFunctions
{
    private static IConfigManager _configManager;
    private static ConfigurationMap _configurationMap;
    private static ServerConfiguration _serverConfiguration;
    private static HouseConfiguration _houseConfiguration;

    public ConfigFunctions(
        IConfigManager configManager,
        ConfigurationMap configurationMap,
        ServerConfiguration serverConfiguration,
        HouseConfiguration houseConfiguration) : base(nameof(ConfigFunctions))
    {
        _configManager = configManager;
        _configurationMap = configurationMap;
        _serverConfiguration = serverConfiguration;
        _houseConfiguration = houseConfiguration ?? new HouseConfiguration();
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
        RegisterVariable(luaState, "configKeys", "HOUSE_PRICE", IntegerConfigType.HOUSE_PRICE_PER_SQM);

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
        var key = GetNumber<IntegerConfigType>(luaState, -1);
        if (key == IntegerConfigType.HOUSE_PRICE_PER_SQM)
        {
            Lua.PushNumber(luaState, _houseConfiguration.PricePerSqm);
            return 1;
        }

        Lua.PushNumber(luaState, _configManager.GetNumber(key));
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