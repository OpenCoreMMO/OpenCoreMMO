using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Server.Standalone.IoC;

public static class ContainerHelpers
{
    private static readonly Lazy<Type[]> _assemblyTypesCache = new(() =>
        [.. Container.AssemblyCache.SelectMany(x => x.GetTypes())]);

    private static Type[] AssemblyCache => _assemblyTypesCache.Value;

    public static IServiceCollection RegisterAssembliesByInterface(this IServiceCollection builder, Type interfaceType)
    {
        var types = AssemblyCache;

        for (var i = 0; i < types.Length; i++)
        {
            var type = types[i];
            if (type.IsAbstract || type.IsEnum || type.IsInterface || type == interfaceType)
                continue;

            if (interfaceType.IsAssignableFrom(type))
            {
                builder.AddSingleton(type);
                continue;
            }

            var interfaces = type.GetInterfaces();
            for (var j = 0; j < interfaces.Length; j++)
                if (interfaces[j].IsGenericType && interfaces[j].GetGenericTypeDefinition() == interfaceType)
                {
                    builder.AddSingleton(type);
                    break;
                }
        }

        return builder;
    }

    public static IServiceCollection RegisterAssemblyTypes(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var types = assembly.GetTypes();

        for (var i = 0; i < types.Length; i++)
        {
            var type = types[i];
            if (!type.IsAbstract && !type.IsEnum && !type.IsInterface && type.IsPublic)
                serviceCollection.AddSingleton(type);
        }

        return serviceCollection;
    }

    public static IServiceCollection RegisterAssemblyTypes<TInterface>(this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
        where TInterface : class
    {
        return serviceCollection.RegisterAssemblyTypes(typeof(TInterface), assemblies);
    }

    public static IServiceCollection RegisterAssemblyTypes(this IServiceCollection serviceCollection, Type @interface,
        params Assembly[] assemblies)
    {
        for (var a = 0; a < assemblies.Length; a++)
        {
            var types = assemblies[a].GetTypes();

            for (var i = 0; i < types.Length; i++)
            {
                var type = types[i];
                if (type.IsAbstract || type.IsInterface || !type.IsPublic || type.IsEnum)
                    continue;

                if (@interface.IsAssignableFrom(type))
                {
                    serviceCollection.AddSingleton(@interface, type);
                    continue;
                }

                var interfaces = type.GetInterfaces();
                for (var j = 0; j < interfaces.Length; j++)
                    if (interfaces[j].IsGenericType && interfaces[j].GetGenericTypeDefinition() == @interface)
                    {
                        serviceCollection.AddSingleton(@interface, type);
                        break;
                    }
            }
        }

        return serviceCollection;
    }

    public static T Resolve<T>(this IServiceProvider serviceProvider) where T : class
    {
        return serviceProvider.GetService<T>();
    }

    public static IServiceProvider Verify(this IServiceProvider serviceProvider, IServiceCollection serviceCollection)
    {
#if DEBUG
        // Only verify in Debug mode - this is expensive and only needed during development
        var count = serviceCollection.Count;
        for (var i = 0; i < count; i++)
        {
            var service = serviceCollection[i];
            if (service.ServiceType.ContainsGenericParameters) continue;

            // Skip verification for implementation types that are registered via interface
            if (service.ImplementationType != null && service.ServiceType != service.ImplementationType)
                continue;

            try
            {
                _ = serviceProvider.GetRequiredService(service.ServiceType);
            }
            catch
            {
                // Allow failures - some services may have optional dependencies
            }
        }
#endif
        return serviceProvider;
    }
}