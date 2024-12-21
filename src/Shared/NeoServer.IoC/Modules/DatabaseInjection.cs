//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using NeoServer.Data.Contexts;
//using NeoServer.Data.Factory;
//using NeoServer.Data.Interfaces;
//using NeoServer.Data.Providers.InMemory;
//using NeoServer.Data.Providers.PostgreSQL;
//using NeoServer.Data.Providers.SQLite;
//using NeoServer.Data.Repositories;
//using NeoServer.Data.Repositories.Player;
//using NeoServer.Server.Configurations;
//using Serilog;

//namespace NeoServer.Shared.IoC.Modules;

//public static class DatabaseInjection
//{
//    public static IServiceCollection AddRepositories(this IServiceCollection builder)
//    {
//        builder.AddSingleton<IAccountRepository, AccountRepository>();
//        builder.AddSingleton<IGuildRepository, GuildRepository>();
//        builder.AddSingleton<IPlayerDepotItemRepository, PlayerDepotItemRepository>();
//        builder.AddSingleton<IPlayerRepository, PlayerRepository>();
//        builder.AddSingleton(typeof(BaseRepository<>));

//        return builder;
//    }

//    public static IServiceCollection AddDatabases(this IServiceCollection builder, IConfiguration configuration)
//    {
//        return builder.RegisterContext<NeoContext>(configuration);
//    }

//    private static IServiceCollection RegisterContext<TContext>(this IServiceCollection builder,
//        IConfiguration configuration)
//        where TContext : DbContext
//    {
//        DatabaseConfiguration config = new(null, DatabaseType.INMEMORY);

//        configuration.GetSection("database").Bind(config);

//        LoadEnvironmentVariables(ref config);

//        var options = config.Active switch
//        {
//            DatabaseType.INMEMORY => DbContextFactory.GetInstance()
//                .UseInMemory(config.Connections[DatabaseType.INMEMORY]),
//            DatabaseType.POSTGRESQL => DbContextFactory.GetInstance()
//                .UsePostgreSQL(config.Connections[DatabaseType.POSTGRESQL]),
//            DatabaseType.SQLITE => DbContextFactory.GetInstance()
//                .UseSQLite(config.Connections[DatabaseType.SQLITE]),
//            _ => throw new ArgumentException("Invalid active database!")
//        };

//        builder.AddSingleton(config);
//        builder.AddSingleton(options);

//        builder.AddSingleton(x => new NeoContext(options, x.GetRequiredService<ILogger>()) as TContext);

//        return builder;
//    }

//    private static void LoadEnvironmentVariables(ref DatabaseConfiguration databaseConfiguration)
//    {
//        var activeDatabase = Environment.GetEnvironmentVariable("ACTIVE_DATABASE");
//        var sqliteDatabase = Environment.GetEnvironmentVariable("SQLITE_DATABASE");
//        var postgresDatabase = Environment.GetEnvironmentVariable("POSTGRESQL_DATABASE");
//        var postgresUser = Environment.GetEnvironmentVariable("POSTGRESQL_USER");
//        var postgresPassword = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD");
//        var postgresHost = Environment.GetEnvironmentVariable("POSTGRESQL_HOST");
//        var postgresPort = Environment.GetEnvironmentVariable("POSTGRESQL_PORT");

//        var dbConnections = databaseConfiguration.Connections;

//        if(!string.IsNullOrEmpty(sqliteDatabase))
//            dbConnections[DatabaseType.SQLITE] = $"Data Source={sqliteDatabase}";

//        if (!string.IsNullOrEmpty(postgresDatabase))
//            dbConnections[DatabaseType.POSTGRESQL] = $"host={postgresHost};port={postgresPort};database={postgresDatabase};username={postgresUser};password={postgresPassword};";
//        var active = databaseConfiguration.Active;

//        if (!string.IsNullOrEmpty(activeDatabase) && Enum.TryParse(activeDatabase, out DatabaseType databaseType))
//            active = databaseType;

//        databaseConfiguration = new(dbConnections, active);
//    }
//}