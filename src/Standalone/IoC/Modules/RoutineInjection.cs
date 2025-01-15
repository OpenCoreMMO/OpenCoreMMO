using Microsoft.Extensions.DependencyInjection;
using NeoServer.Server.Routines;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class RoutineInjection
{
    public static IServiceCollection AddRoutines(this IServiceCollection builder)
    {
        return builder.RegisterAssembliesByInterface(typeof(IRoutine));
    }
}