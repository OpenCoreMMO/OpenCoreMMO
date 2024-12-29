using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.Lua;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Server.Common.Contracts.Scripts;
using NLua;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class LuaInjection
{
    public static IServiceCollection AddLua(this IServiceCollection builder)
    {
        builder.AddSingleton(new Lua());
        builder.AddSingleton<LuaGlobalRegister>();

        //LuaJIT
        builder.AddSingleton<IScriptGameManager, LuaScriptGameManager>();

        return builder;
    }
}