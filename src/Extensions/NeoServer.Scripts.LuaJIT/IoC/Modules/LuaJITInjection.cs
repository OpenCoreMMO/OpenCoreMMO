using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.LuaJIT.Functions;
using NeoServer.Scripts.LuaJIT.Functions.Interfaces;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.IoC.Modules;

public static class LuaJITInjection
{
    public static IServiceCollection AddLuaJIT(this IServiceCollection builder)
    {
        builder.AddSingleton<IConfigManager, ConfigManager>();
        builder.AddSingleton<ILuaEnvironment, LuaEnvironment>();
        builder.AddSingleton<IScripts, Scripts>();
        builder.AddSingleton<IActions, Actions>();
        builder.AddSingleton<ITalkActions, TalkActions>();

        builder.AddSingleton<IActionFunctions, ActionFunctions>();
        builder.AddSingleton<IConfigFunctions, ConfigFunctions>();
        builder.AddSingleton<IContainerFunctions, ContainerFunctions>();
        builder.AddSingleton<ICreatureFunctions, CreatureFunctions>();
        builder.AddSingleton<IEnumFunctions, EnumFunctions>();
        builder.AddSingleton<IGameFunctions, GameFunctions>();
        builder.AddSingleton<IGlobalFunctions, GlobalFunctions>();
        builder.AddSingleton<IItemFunctions, ItemFunctions>();
        builder.AddSingleton<IItemTypeFunctions, ItemTypeFunctions>();
        builder.AddSingleton<ILoggerFunctions, LoggerFunctions>();
        builder.AddSingleton<IMonsterFunctions, MonsterFunctions>();
        builder.AddSingleton<INpcFunctions, NpcFunctions>();
        builder.AddSingleton<IPlayerFunctions, PlayerFunctions>();
        builder.AddSingleton<IPositionFunctions, PositionFunctions>();
        builder.AddSingleton<ITalkActionFunctions, TalkActionFunctions>();
        builder.AddSingleton<ITeleportFunctions, TeleportFunctions>();
        builder.AddSingleton<ITileFunctions, TileFunctions>();

        return builder;
    }
}