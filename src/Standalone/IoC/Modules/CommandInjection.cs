using Microsoft.Extensions.DependencyInjection;
using NeoServer.Server.Commands.Movements.ToContainer;
using NeoServer.Server.Commands.Movements.ToInventory;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class CommandInjection
{
    public static IServiceCollection AddCommands(this IServiceCollection builder)
    {
        builder.AddSingleton<MapToContainerMovementOperation>();
        builder.AddSingleton<MapToInventoryMovementOperation>();

        // Guild commands are now handled by Lua scripts
        return builder;
    }
}