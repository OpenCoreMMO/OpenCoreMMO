using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoServer.Game.Common;
using NeoServer.Server.Configurations;

namespace NeoServer.Server.Standalone.IoC.Modules;

public static class ConfigurationInjection
{
    public static IConfigurationRoot GetConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables();

        //only add secrets in development
        if (environmentName != null && environmentName.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
            builder.AddUserSecrets<Program>();
        return builder.Build();
    }

    public static IServiceCollection AddConfigurations(this IServiceCollection builder,
        IConfigurationRoot configuration)
    {
        ServerConfiguration serverConfiguration =
            new(0, null, null, null, string.Empty, string.Empty, string.Empty, 7171, 7172, new SaveConfiguration(3600));
        GameConfiguration gameConfiguration = new();
        LogConfiguration logConfiguration = new(null);
        ClientConfiguration clientConfiguration = new(null);

        configuration.GetSection("server").Bind(serverConfiguration);
        configuration.GetSection("game").Bind(gameConfiguration);
        configuration.GetSection("log").Bind(logConfiguration);
        configuration.GetSection("client").Bind(clientConfiguration);

        LoadEnvironmentVariables(ref serverConfiguration);

        builder.AddSingleton(serverConfiguration);
        builder.AddSingleton(gameConfiguration);
        builder.AddSingleton(logConfiguration);
        builder.AddSingleton(clientConfiguration);

        return builder;
    }

    private static void LoadEnvironmentVariables(ref ServerConfiguration serverConfiguration)
    {
        var serverLoginPort = Environment.GetEnvironmentVariable("SERVER_LOGIN_PORT");
        var serverGamePort = Environment.GetEnvironmentVariable("SERVER_GAME_PORT");
        var serverGameName = Environment.GetEnvironmentVariable("SERVER_GAME_NAME");
        var serverGameIP = Environment.GetEnvironmentVariable("SERVER_GAME_IP");

        serverConfiguration = new(
            serverConfiguration.Version,
            serverConfiguration.OTBM,
            serverConfiguration.OTB,
            serverConfiguration.Data,
            string.IsNullOrEmpty(serverGameName) ? serverConfiguration.ServerName : serverGameName,
            string.IsNullOrEmpty(serverGameIP) ? serverConfiguration.ServerIp : serverGameIP,
            serverConfiguration.Extensions,
            string.IsNullOrEmpty(serverLoginPort) ? serverConfiguration.ServerLoginPort : int.Parse(serverLoginPort),
            string.IsNullOrEmpty(serverGamePort) ? serverConfiguration.ServerGamePort : int.Parse(serverGamePort),
            serverConfiguration.Save);
    }
}