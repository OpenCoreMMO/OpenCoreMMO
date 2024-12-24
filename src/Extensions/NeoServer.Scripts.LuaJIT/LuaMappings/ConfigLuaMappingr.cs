using LuaNET;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;
using NeoServer.Server.Configurations;

namespace NeoServer.Scripts.LuaJIT.LuaMappings;

public class ConfigLuaMappingr : LuaScriptInterface, IConfigLuaMapping
{
    private static IConfigManager _configManager;
    private ServerConfiguration _serverConfiguration;

    public ConfigLuaMappingr(
        IConfigManager configManager,
        ServerConfiguration serverConfiguration) : base(nameof(ConfigLuaMappingr))
    {
        _configManager = configManager;
        _serverConfiguration = serverConfiguration;
    }

    public void Init(LuaState L)
    {
        RegisterTable(L, "configManager");
        RegisterMethod(L, "configManager", "getString", LuaConfigManagerGetString);
        RegisterMethod(L, "configManager", "getNumber", LuaConfigManagerGetNumber);
        RegisterMethod(L, "configManager", "getBoolean", LuaConfigManagerGetBoolean);
        RegisterMethod(L, "configManager", "getFloat", LuaConfigManagerGetFloat);

        RegisterTable(L, "configKeys");

        //RegisterVariable(L, "configKeys", "ALLOW_CHANGEOUTFIT", BooleanConfig.ALLOW_CHANGEOUTFIT);

        foreach (var item in Enum.GetValues<BooleanConfigType>())
            RegisterVariable(L, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<StringConfigType>())
            RegisterVariable(L, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<IntegerConfigType>())
            RegisterVariable(L, "configKeys", item.ToString(), item);

        foreach (var item in Enum.GetValues<FloatingConfigType>())
            RegisterVariable(L, "configKeys", item.ToString(), item);

        RegisterVariable(L, "configKeys", "BASE_DIRECTORY", AppContext.BaseDirectory + _serverConfiguration.DataLuaJit);
    }

    private static int LuaConfigManagerGetString(LuaState L)
    {
        // configManager:getString()
        PushString(L, _configManager.GetString(GetNumber<StringConfigType>(L, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetNumber(LuaState L)
    {
        // configManager:getNumber()
        Lua.PushNumber(L, _configManager.GetNumber(GetNumber<IntegerConfigType>(L, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetBoolean(LuaState L)
    {
        // configManager:getBoolean()
        PushBoolean(L, _configManager.GetBoolean(GetNumber<BooleanConfigType>(L, -1)));
        return 1;
    }

    private static int LuaConfigManagerGetFloat(LuaState L)
    {
        // configManager:getFloat()
        Lua.PushNumber(L, _configManager.GetFloat(GetNumber<FloatingConfigType>(L, -1)));
        return 1;
    }
}
