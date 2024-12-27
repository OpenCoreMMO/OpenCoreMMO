using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Scripts.LuaJIT.LuaMappings;
using NeoServer.Scripts.LuaJIT.LuaMappings.Interfaces;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class LuaJITInjection
{
    public static IServiceCollection AddLuaJIT(this IServiceCollection builder)
    {
        builder.AddSingleton<IConfigManager, ConfigManager>();
        builder.AddSingleton<ILuaEnvironment, LuaEnvironment>();
        builder.AddSingleton<IScripts, Scripts.LuaJIT.Scripts>();
        builder.AddSingleton<IActions, Actions>();
        builder.AddSingleton<ITalkActions, TalkActions>();

        builder.AddSingleton<IActionLuaMapping, ActionLuaMapping>();
        builder.AddSingleton<IConfigLuaMapping, ConfigLuaMappingr>();
        builder.AddSingleton<ICreatureLuaMapping, CreatureLuaMapping>();
        builder.AddSingleton<IEnumLuaMapping, EnumLuaMapping>();
        builder.AddSingleton<IGameLuaMapping, GameLuaMapping>();
        builder.AddSingleton<IGlobalLuaMapping, GlobalLuaMapping>();
        builder.AddSingleton<IItemLuaMapping, ItemLuaMapping>();
        builder.AddSingleton<IItemTypeLuaMapping, ItemTypeLuaMapping>();
        builder.AddSingleton<ILoggerLuaMapping, LoggerLuaMapping>();
        builder.AddSingleton<IMonsterLuaMapping, MonsterLuaMapping>();
        builder.AddSingleton<IPlayerLuaMapping, PlayerLuaMapping>();
        builder.AddSingleton<IPositionLuaMapping, PositionLuaMapping>();
        builder.AddSingleton<ITalkActionLuaMapping, TalkActionLuaMapping>();
        builder.AddSingleton<ITileLuaMapping, TileLuaMapping>();

        return builder;
    }
}