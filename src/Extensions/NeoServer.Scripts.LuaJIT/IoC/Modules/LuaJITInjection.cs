using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.LuaJIT;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class LuaJITInjection
{
    public static IServiceCollection AddLuaJIT(this IServiceCollection builder)
    {
        //LuaJIT todo: move this to lUaScripts
        builder.AddSingleton<IConfigManager, ConfigManager>();
        builder.AddSingleton<ILuaEnvironment, LuaEnvironment>();
        builder.AddSingleton<IScripts, Scripts.LuaJIT.Scripts>();
        builder.AddSingleton<ITalkActions, TalkActions>();
        builder.AddSingleton<ICreatureFunctions, CreatureFunctions>();
        //
        return builder;
    }
}