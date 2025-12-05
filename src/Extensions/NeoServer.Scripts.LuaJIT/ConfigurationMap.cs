using Microsoft.Extensions.Configuration;
using NeoServer.Scripts.LuaJIT.Enums;

namespace NeoServer.Scripts.LuaJIT;

/// <summary>
/// Configuration map for Lua scripts
/// </summary>
public class ConfigurationMap(IConfiguration configuration)
{
    public static readonly Dictionary<BooleanConfigType, string> BooleanMap = new()
    {
        [BooleanConfigType.REMOVE_POTION_CHARGES] = "game:enablePotionCharges"
    };

    public bool GetBoolean(BooleanConfigType config) => BooleanMap.TryGetValue(config, out var configKey) && configuration.GetValue(configKey, defaultValue: false);
}