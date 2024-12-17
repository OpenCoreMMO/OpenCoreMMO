using Serilog;
using Serilog.Sinks.Graylog;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog.Settings.Configuration;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Sinks.Graylog.Core.Transport;

public static class LoggerConfigurationExtensions
{
    public static ContainerBuilder AddLogger(this ContainerBuilder builder, IConfiguration configuration)
    {
        var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly)
        {
            SectionName = "Log"
        };

        var graylogConfig = configuration.GetSection("GrayLog");

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, options)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .WriteTo.Graylog(new GraylogSinkOptions
            {
                HostnameOrAddress = graylogConfig.GetValue<string>("HostnameOrAddress"),
                Port = graylogConfig.GetValue<int>("Port"),
                TransportType = TransportType.Tcp,
                Facility = graylogConfig.GetValue<string>("Facility"),
                UseSsl = false,
                HostnameOverride = graylogConfig.GetValue<string>("HostnameOverride"),
            });

        var logger = loggerConfig.CreateLogger();

        builder.RegisterInstance(logger).As<ILogger>().SingleInstance();
        builder.RegisterInstance(loggerConfig).SingleInstance();

        return builder;
    }
}
