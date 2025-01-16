using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.Lua;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Scripts.LuaJIT.Services;
using NeoServer.Server.Common.Contracts.Scripts;
using NeoServer.Server.Common.Contracts.Scripts.Services;
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
        builder.AddSingleton<IActionScripts, LuaActionScripts>();
        builder.AddSingleton<ICreatureEventsScripts, LuaCreatureEventsScripts>();
        builder.AddSingleton<IGlobalEventsScripts, LuaGlobalEventsScripts>();
        builder.AddSingleton<IMoveEventsScripts, LuaMoveEventsScripts>();
        builder.AddSingleton<ITalkActionScripts, LuaTalkActionScripts>();

        return builder;
    }
}