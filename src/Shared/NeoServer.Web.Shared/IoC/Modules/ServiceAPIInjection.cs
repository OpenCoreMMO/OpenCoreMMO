using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NeoServer.Web.API.Helpers;

namespace NeoServer.Web.API.IoC.Modules;

public static class ServiceApiInjection
{
    public static IServiceCollection AddServicesApi(this IServiceCollection services)
    {
        var scanAssemblies = AssemblyHelper.GetAllAssemblies();

        var servicesAndRepositories = scanAssemblies
            .SelectMany(o => o.DefinedTypes
                .Where(x => x.IsInterface)
                .Where(c => c.FullName?.EndsWith("ApiService") ?? false)
            );

        foreach (var typeInfo in servicesAndRepositories)
        {
            var types = scanAssemblies
                .SelectMany(o => o.DefinedTypes
                    .Where(x => x.IsClass)
                    .Where(x => typeInfo.IsAssignableFrom(x))
                );

            foreach (var type in types)
                services.TryAdd(new ServiceDescriptor(typeInfo, type, ServiceLifetime.Scoped));
        }

        return services;
    }
}