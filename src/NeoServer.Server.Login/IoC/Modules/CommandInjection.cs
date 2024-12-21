using Microsoft.Extensions.DependencyInjection;
using NeoServer.Server.Commands.Movements.ToContainer;
using NeoServer.Server.Commands.Movements.ToInventory;

namespace NeoServer.Server.Login.IoC.Modules;

public static class CommandInjection
{
    public static IServiceCollection AddCommands(this IServiceCollection builder)
    {
        builder.AddSingleton<MapToContainerMovementOperation>();
        builder.AddSingleton<MapToInventoryMovementOperation>();
        return builder;
    }
}