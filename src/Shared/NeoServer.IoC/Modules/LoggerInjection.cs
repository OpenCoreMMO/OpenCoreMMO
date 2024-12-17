
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Settings.Configuration;
using Serilog.Sinks.SystemConsole.Themes;

namespace NeoServer.Shared.IoC.Modules;

public static class LoggerInjection
{
    public static IServiceCollection AddLogger(this IServiceCollection builder, IConfiguration configuration)
    {
        var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly)
        {
            SectionName = "Log"
        };

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, options)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code);

        var logger = loggerConfig.CreateLogger();

        builder.AddSingleton<ILogger>(logger);
        builder.AddSingleton(loggerConfig);
        return builder;
    }
}