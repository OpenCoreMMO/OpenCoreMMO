using Microsoft.Extensions.DependencyInjection;
using NeoServer.Application.Common.Contracts.Scripts;
using NeoServer.Scripts.Lua;
using NeoServer.Scripts.LuaJIT;
using NLua;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class LuaInjection
{
    public static IServiceCollection AddLua(this IServiceCollection builder)
    {
        builder.AddSingleton(new Lua());
        builder.AddSingleton<LuaGlobalRegister>();


        //LuaJIT todo: move this to lUaScripts
        builder.AddSingleton<IConfigManager, ConfigManager>();
        builder.AddSingleton<ILuaManager, LuaManager>();
        builder.AddSingleton<ILuaEnvironment, LuaEnvironment>();
        builder.AddSingleton<IScripts, NeoServer.Scripts.LuaJIT.Scripts>();
        builder.AddSingleton<ITalkActions, TalkActions>();
        builder.AddSingleton<ICreatureFunctions, CreatureFunctions>();
        //
        return builder;
    }
}