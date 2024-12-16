using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NeoServer.Data.Contexts;
using NeoServer.Data.Factory;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Providers.InMemory;
using NeoServer.Data.Providers.PostgreSQL;
using NeoServer.Data.Providers.SQLite;
using NeoServer.Data.Repositories;
using NeoServer.Data.Repositories.Player;
using NeoServer.Server.Configurations;

namespace NeoServer.Shared.IoC.Modules;

public static class DatabaseInjection
{
    public static ContainerBuilder AddRepositories(this ContainerBuilder builder)
    {
        builder.RegisterType<AccountRepository>().As<IAccountRepository>().SingleInstance();
        builder.RegisterType<GuildRepository>().As<IGuildRepository>().SingleInstance();
        builder.RegisterType<PlayerDepotItemRepository>().As<IPlayerDepotItemRepository>().SingleInstance();
        builder.RegisterType<PlayerRepository>().As<IPlayerRepository>().SingleInstance();
        builder.RegisterGeneric(typeof(BaseRepository<>));

        return builder;
    }

    public static ContainerBuilder AddDatabases(this ContainerBuilder builder, IConfiguration configuration)
    {
        builder.RegisterContext<NeoContext>(configuration);
        return builder;
    }

    private static void RegisterContext<TContext>(this ContainerBuilder builder, IConfiguration configuration)
        where TContext : DbContext
    {
        DatabaseConfiguration config = new(null, DatabaseType.INMEMORY);

        configuration.GetSection("database").Bind(config);

        var options = config.Active switch
        {
            DatabaseType.INMEMORY => DbContextFactory.GetInstance()
                .UseInMemory(config.Connections[DatabaseType.INMEMORY]),
            DatabaseType.MONGODB => DbContextFactory.GetInstance()
                .UseInMemory(config.Connections[DatabaseType.MONGODB]),
            DatabaseType.POSTGRESQL => DbContextFactory.GetInstance().UsePostgreSQL(config.Connections[DatabaseType.POSTGRESQL]),
            DatabaseType.MSSQL => DbContextFactory.GetInstance()
                .UseInMemory(config.Connections[DatabaseType.MSSQL]),
            DatabaseType.SQLITE => DbContextFactory.GetInstance()
                .UseSQLite(config.Connections[DatabaseType.SQLITE]),
            _ => throw new ArgumentException("Invalid active database!")
        };

        builder.RegisterInstance(config).SingleInstance();

        builder.RegisterType<TContext>()
            .WithParameter("options", options)
            .InstancePerLifetimeScope();

        builder.RegisterInstance(options).SingleInstance();
    }
}