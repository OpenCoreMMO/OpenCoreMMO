//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using NeoServer.Server.Configurations;
//using Serilog;
//using Serilog.Settings.Configuration;
//using Serilog.Sinks.Graylog;
//using Serilog.Sinks.Graylog.Core.Transport;
//using Serilog.Sinks.SystemConsole.Themes;
//using static Org.BouncyCastle.Math.EC.ECCurve;

//public static class LoggerConfigurationExtensions
//{
//    public static IServiceCollection AddLogger(this IServiceCollection builder, IConfiguration configuration)
//    {
//        var options = new ConfigurationReaderOptions(typeof(ConsoleLoggerConfigurationExtensions).Assembly)
//        {
//            SectionName = "Log"
//        };

//        var grayLogConfiguration = new GrayLogConfiguration(string.Empty, 0, string.Empty, string.Empty);

//        configuration.GetSection("GrayLog").Bind(grayLogConfiguration);

//        LoadEnvironmentVariables(ref grayLogConfiguration);

//        var loggerConfig = new LoggerConfiguration()
//            .ReadFrom.Configuration(configuration, options)
//            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
//            .WriteTo.Graylog(new GraylogSinkOptions
//            {
//                HostnameOrAddress = grayLogConfiguration.HostnameOrAddress,
//                Port = grayLogConfiguration.Port,
//                TransportType = TransportType.Tcp,
//                Facility = grayLogConfiguration.Facility,
//                UseSsl = false,
//                HostnameOverride = grayLogConfiguration.HostnameOverride
//            });

//        var logger = loggerConfig.CreateLogger();

//        builder.AddSingleton<ILogger>(logger);
//        builder.AddSingleton(loggerConfig);
//        return builder;
//    }

//    private static void LoadEnvironmentVariables(ref GrayLogConfiguration grayLogConfiguration)
//    {
//        var graylogHostnameOrAddress = Environment.GetEnvironmentVariable("GRAYLOG_HOSTNAME_OR_ADDRESS");
//        var graylogPort = Environment.GetEnvironmentVariable("GRAYLOG_PORT");
//        var graylogHostnameOverride = Environment.GetEnvironmentVariable("GRAYLOG_HOSTNAME_OVERRIDE");
//        var graylogFacility = Environment.GetEnvironmentVariable("GRAYLOG_FACILITY");

//        grayLogConfiguration = new(
//            string.IsNullOrEmpty(graylogHostnameOrAddress) ? grayLogConfiguration.HostnameOrAddress : graylogHostnameOrAddress,
//            string.IsNullOrEmpty(graylogPort) ? grayLogConfiguration.Port : int.Parse(graylogPort),
//            string.IsNullOrEmpty(graylogHostnameOverride) ? grayLogConfiguration.HostnameOverride : graylogHostnameOverride,
//            string.IsNullOrEmpty(graylogFacility) ? grayLogConfiguration.Facility : graylogFacility);
//    }
//}