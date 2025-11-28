using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Shared.IoC.Modules;

public static class GuildCommandInjection
{
    public static IServiceCollection AddGuildCommands(this IServiceCollection builder)
    {
        // Guild system now uses Lua scripts instead of C# commands
        return builder;
    }
}