using Microsoft.Extensions.DependencyInjection;
using NeoServer.Scripts.Lua;
using NeoServer.Scripts.LuaJIT;
using NeoServer.Scripts.LuaJIT.ScriptServices;
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
        builder.AddSingleton<IScriptManager, LuaScriptManager>();
        builder.AddSingleton<IActionScriptService, LuaActionScriptService>();
        builder.AddSingleton<ICreatureEventsScriptService, LuaCreatureEventsScriptService>();
        builder.AddSingleton<IGlobalEventsScriptService, LuaGlobalEventsScriptService>();
        builder.AddSingleton<IMoveEventsScriptService, LuaMoveEventsScriptService>();
        builder.AddSingleton<ITalkActionScriptService, LuaTalkActionScriptService>();

        return builder;
    }
}