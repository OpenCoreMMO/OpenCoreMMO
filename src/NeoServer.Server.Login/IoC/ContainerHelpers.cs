using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NeoServer.Server.Login.IoC;

public static class ContainerHelpers
{
    private static Type[] AssemblyCache => Container.AssemblyCache.SelectMany(x => x.GetTypes()).ToArray();

    public static IServiceCollection RegisterAssembliesByInterface(this IServiceCollection builder, Type interfaceType)
    {
        var types = AssemblyCache
            .Where(x => interfaceType.IsAssignableFrom(x) || x.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType))
            .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface);

        foreach (var type in types)
        {
            if (type == interfaceType) continue;
            builder.AddSingleton(type);
        }

        return builder;
    }

    public static IServiceCollection RegisterAssemblyTypes(this IServiceCollection serviceCollection, Assembly assembly)
    {
        var types = assembly.GetTypes().Where(t =>
                !t.IsAbstract &&
                !t.IsEnum &&
                !t.IsInterface && t.IsPublic)
            .ToList();

        types.ForEach(t => serviceCollection.AddSingleton(t));
        return serviceCollection;
    }

    public static IServiceCollection RegisterAssemblyTypes<TInterface>(this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
        where TInterface : class
    {
        return RegisterAssemblyTypes(serviceCollection, typeof(TInterface), assemblies);
    }

    public static IServiceCollection RegisterAssemblyTypes(this IServiceCollection serviceCollection, Type @interface,
        params Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(x => x.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic && !t.IsEnum)
            .Where(x => @interface.IsAssignableFrom(x) || x.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == @interface))
            .ToList();

        types.ForEach(t => serviceCollection.AddSingleton(@interface, t));
        return serviceCollection;
    }

    public static T Resolve<T>(this IServiceProvider serviceProvider) where T : class
    {
        return serviceProvider.GetService<T>();
    }

    public static IServiceProvider Verify(this IServiceProvider serviceProvider, IServiceCollection serviceCollection)
    {
        foreach (var service in serviceCollection)
        {
            if (service.ServiceType.ContainsGenericParameters) continue;
            _ = serviceProvider.GetRequiredService(service.ServiceType);
        }

        return serviceProvider;
    }
}