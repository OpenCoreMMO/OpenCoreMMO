using Microsoft.Extensions.DependencyInjection;
using NeoServer.Loaders.Interfaces;
using NeoServer.Loaders.Items;
using NeoServer.Loaders.Monsters;
using NeoServer.Loaders.Quest;
using NeoServer.Loaders.Spawns;
using NeoServer.Loaders.Spells;
using NeoServer.Loaders.Vocations;
using NeoServer.Loaders.World;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Login.IoC.Modules;

public static class LoaderInjection
{
    public static IServiceCollection AddLoaders(this IServiceCollection builder)
    {
        builder.AddSingleton<ItemTypeLoader>();
        builder.AddSingleton<WorldLoader>();
        builder.AddSingleton<SpawnLoader>();
        builder.AddSingleton<MonsterLoader>();
        builder.AddSingleton<VocationLoader>();
        builder.RegisterPlayerLoaders();
        builder.RegisterStartupLoaders();
        builder.AddSingleton<SpellLoader>();
        builder.AddSingleton<QuestDataLoader>();

        builder.RegisterCustomLoaders();

        builder
            .RegisterAssemblyTypes<IRunBeforeLoaders>(Container.AssemblyCache)
            .RegisterAssemblyTypes<IStartup>(Container.AssemblyCache);

        return builder;
    }

    private static void RegisterPlayerLoaders(this IServiceCollection builder)
    {
        var types = Container.AssemblyCache;
        builder.RegisterAssemblyTypes<IPlayerLoader>(types);
    }

    private static void RegisterStartupLoaders(this IServiceCollection builder)
    {
        var types = Container.AssemblyCache;
        builder.RegisterAssemblyTypes<IStartupLoader>(types);
    }

    private static void RegisterCustomLoaders(this IServiceCollection builder)
    {
        builder.RegisterAssembliesByInterface(typeof(ICustomLoader));
    }
}